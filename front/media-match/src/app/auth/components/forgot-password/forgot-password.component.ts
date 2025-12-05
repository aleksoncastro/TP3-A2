import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router, RouterModule } from '@angular/router';
import { AuthService, ForgotPasswordRequestDto } from '../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    RouterModule,
  ],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
})
export class ForgotPasswordComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private sb = inject(MatSnackBar);
  private router = inject(Router);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  loading = false;

  onSubmit() {
    if (this.loading) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.auth.requestPasswordReset(this.form.value as ForgotPasswordRequestDto).subscribe({
      next: () => {
        this.loading = false;
        this.sb.open('Enviamos um código para o seu email', 'fechar', { duration: 4000 });
        const email = this.form.controls.email.value ?? '';
        const target = `/auth/reset-password${email ? `?email=${encodeURIComponent(email)}` : ''}`;
        this.router.navigateByUrl(target);
      },
      error: (err) => {
        this.loading = false;
        this.sb.open(String(err?.error ?? 'Não foi possível enviar o código'), 'fechar', { duration: 4000 });
      },
    });
  }

  gotoLogin() {
    this.router.navigateByUrl('/auth/login');
  }
}
