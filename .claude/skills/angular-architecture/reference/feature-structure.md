> ⚠ **PARCIALMENTE DESATUALIZADO (ADR-0009).** As seções de DTO, página, rota e a divisão
> smart/dumb continuam válidas. Já o **api service está desatualizado**: usa `Observable` +
> `toSignal`, e o padrão agora é `httpResource` para LEITURA (`HttpClient` só na escrita).
> Não copie o bloco do api service sem converter.

# Template canônico — Feature Angular (service, página, rota)

Copie a estrutura. Standalone + signals + inject().

## DTOs — valores monetários em centavos (`number`), como no backend

```typescript
// features/wallet/data/wallet.models.ts
export interface WalletView {
  walletId: string;
  currency: string;
  balanceCents: number; // centavos, igual ao backend. Formatação é na exibição.
}

export interface WithdrawRequest {
  amountCents: number;
}
```

## API service — único ponto que fala com HttpClient

```typescript
// features/wallet/data/wallet.api.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { WalletView, WithdrawRequest } from './wallet.models';

@Injectable({ providedIn: 'root' })
export class WalletApiService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/wallets';

  getById(id: string): Observable<WalletView> {
    return this.http.get<WalletView>(`${this.base}/${id}`);
  }

  withdraw(id: string, req: WithdrawRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/withdraw`, req);
  }
}
```

## Página (smart) — mantém estado em signals, orquestra

```typescript
// features/wallet/pages/wallet-details.page.ts
import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { WalletApiService } from '../data/wallet.api.service';
import { WalletBalanceComponent } from '../ui/wallet-balance.component';

@Component({
  selector: 'app-wallet-details',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WalletBalanceComponent],
  template: `
    @if (wallet(); as w) {
      <app-wallet-balance [view]="w" />
    } @else {
      <p>Carregando…</p>
    }
  `,
})
export class WalletDetailsPage {
  private readonly api = inject(WalletApiService);
  readonly id = input.required<string>();

  // RxJS SÓ aqui, na borda de I/O, convertido para signal.
  readonly wallet = toSignal(this.api.getById(this.id()));
}
```

## Componente dumb — só apresentação, zero service

```typescript
// features/wallet/ui/wallet-balance.component.ts
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { WalletView } from '../data/wallet.models';

@Component({
  selector: 'app-wallet-balance',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe],
  template: `<strong>{{ reais() | currency: view().currency }}</strong>`,
})
export class WalletBalanceComponent {
  readonly view = input.required<WalletView>();
  // centavos -> reais só na exibição.
  readonly reais = computed(() => this.view().balanceCents / 100);
}
```

## Rota lazy da feature

```typescript
// features/wallet/wallet.routes.ts
import { Routes } from '@angular/router';

export const walletRoutes: Routes = [
  { path: ':id', loadComponent: () => import('./pages/wallet-details.page').then(m => m.WalletDetailsPage) },
];
```

```typescript
// app.routes.ts
export const routes: Routes = [
  { path: 'wallets', loadChildren: () => import('./features/wallet/wallet.routes').then(m => m.walletRoutes) },
];
```
