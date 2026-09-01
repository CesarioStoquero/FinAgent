# Template canônico — Teste de frontend

Testa COMPORTAMENTO visível (o que o usuário vê/dispara), não implementação.
Toda página e todo service de API têm teste.

## Service de API — HttpTestingController

```typescript
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { WalletApiService } from './wallet.api.service';

describe('WalletApiService', () => {
  let service: WalletApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(WalletApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('withdraw envia centavos para o endpoint correto', () => {
    service.withdraw('w1', { amountCents: 3000 }).subscribe();

    const req = httpMock.expectOne('/api/wallets/w1/withdraw');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ amountCents: 3000 });
    req.flush(null);
  });
});
```

## Componente — testar o que o usuário vê

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WalletBalanceComponent } from './wallet-balance.component';

describe('WalletBalanceComponent', () => {
  let fixture: ComponentFixture<WalletBalanceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [WalletBalanceComponent] }).compileComponents();
    fixture = TestBed.createComponent(WalletBalanceComponent);
  });

  it('exibe o saldo em reais formatado a partir de centavos', () => {
    fixture.componentRef.setInput('view', { walletId: 'w1', currency: 'BRL', balanceCents: 12345 });
    fixture.detectChanges();

    // R$123,45 — comportamento visível, não o computed interno.
    expect(fixture.nativeElement.textContent).toContain('123,45');
  });
});
```

## Erros que a IA comete aqui (evite)

- Assertar sobre método/propriedade privada em vez do DOM/output visível.
- Esquecer `httpMock.verify()` — deixa requisição pendente passar batido.
- `fixture.detectChanges()` faltando após `setInput` — a view não atualiza.
