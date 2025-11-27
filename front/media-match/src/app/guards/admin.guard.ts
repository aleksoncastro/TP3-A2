import { CanMatchFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../auth/services/auth.service';
import { map, of, switchMap } from 'rxjs';

export const AdminGuard: CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const hasRole = auth.getRole();
  if (hasRole) {
    if (auth.isAdmin()) return of(true);
    router.navigateByUrl('/');
    return of(false);
  }
  return auth.me().pipe(
    map((me) => me.role === 'admin'),
    switchMap((ok) => {
      if (!ok) {
        router.navigateByUrl('/');
      }
      return of(ok);
    })
  );
};
