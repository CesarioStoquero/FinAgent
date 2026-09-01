# Template canônico — Reactive Form tipado + erros via signal

Reactive Forms tipado. Mensagens derivadas de signals. Dinheiro em centavos no envio.

```typescript
import { ChangeDetectionStrategy, Component, computed, inject, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { WithdrawRequest } from '../data/wallet.models';

interface WithdrawForm {
  amountReais: FormControl<number | null>;
}

@Component({
  selector: 'app-withdraw-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="submit()">
      <input type="number" formControlName="amountReais" min="0.01" step="0.01" />
      @if (amountError(); as msg) {
        <small class="error">{{ msg }}</small>
      }
      <button type="submit" [disabled]="form.invalid">Sacar</button>
    </form>
  `,
})
export class WithdrawFormComponent {
  readonly confirmed = output<WithdrawRequest>();

  readonly form = new FormGroup<WithdrawForm>({
    amountReais: new FormControl<number | null>(null, {
      validators: [Validators.required, Validators.min(0.01)],
    }),
  });

  // Estado do controle como signal para derivar a mensagem no template.
  private readonly status = signal(this.form.controls.amountReais.status);

  readonly amountError = computed(() => {
    const c = this.form.controls.amountReais;
    if (c.valid || c.pristine) return null;
    if (c.hasError('required')) return 'Informe um valor.';
    if (c.hasError('min')) return 'O valor deve ser maior que zero.';
    return 'Valor inválido.';
  });

  submit(): void {
    if (this.form.invalid) return;
    const reais = this.form.controls.amountReais.value!;
    // Converte reais -> centavos SÓ na saída para o backend.
    this.confirmed.emit({ amountCents: Math.round(reais * 100) });
  }
}
```

## Erros que a IA comete aqui (evite)

- Enviar `amountReais` direto ao backend — o backend só entende centavos (`Math.round(x*100)`).
- `FormGroup` sem tipo genérico (`new FormGroup({...})` solto) — perde a tipagem estrita.
- Mensagem de erro montada no template com lógica — derive num `computed`.
