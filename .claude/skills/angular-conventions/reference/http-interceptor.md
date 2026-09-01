# Template canônico — Interceptor de erro/auth (functional)

Interceptor central: mapeia erro HTTP → mensagem de domínio e injeta auth.
NUNCA trate erro HTTP dentro do componente — é aqui.

```typescript
// core/http/error.interceptor.ts
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notify = inject(NotificationService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      const message = mapToDomainMessage(err);
      notify.error(message);                 // feedback central, um só lugar
      return throwError(() => new Error(message));
    }),
  );
};

function mapToDomainMessage(err: HttpErrorResponse): string {
  switch (err.status) {
    case 0:   return 'Sem conexão com o servidor.';
    case 400: return err.error?.detail ?? 'Requisição inválida.';
    case 404: return 'Recurso não encontrado.';
    case 409: return 'Conflito: o recurso mudou. Tente novamente.'; // ex.: concorrência otimista
    case 422: return err.error?.detail ?? 'Operação não permitida.'; // ex.: saldo insuficiente
    default:  return 'Erro inesperado. Tente novamente.';
  }
}
```

```typescript
// app.config.ts — registro
export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    // ...
  ],
};
```

## Erros que a IA comete aqui (evite)

- `catchError` espalhado em cada `*.api.service.ts` — centralize no interceptor.
- Interceptor baseado em classe (`HttpInterceptor`) — use functional (`HttpInterceptorFn`).
