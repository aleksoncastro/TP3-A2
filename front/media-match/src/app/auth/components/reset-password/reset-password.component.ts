import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService, ResetPasswordRequestDto } from '../../services/auth.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-reset-password',
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
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css'],
})
export class ResetPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private sb = inject(MatSnackBar);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  loading = false;

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email');
    if (email) {
      this.form.patchValue({ email });
    }

    this.form.controls.confirmPassword.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.clearMismatchError());

    this.form.controls.newPassword.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.clearMismatchError());
  }

  onSubmit() {
    if (this.loading) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email, code, newPassword, confirmPassword } = this.form.value;
    if (newPassword !== confirmPassword) {
      this.form.controls.confirmPassword.setErrors({ mismatch: true });
      this.form.controls.confirmPassword.markAsTouched();
      this.sb.open('As senhas não coincidem', 'fechar', { duration: 4000 });
      return;
    }
    this.loading = true;
    const payload: ResetPasswordRequestDto = {
      email: email!,
      code: code!,
      newPassword: newPassword!,
    };

    this.auth.resetPassword(payload).subscribe({
      next: () => {
        this.loading = false;
        this.sb.open('Senha redefinida com sucesso', 'fechar', { duration: 4000 });
        this.router.navigateByUrl('/auth/login');
      },
      error: (err) => {
        this.loading = false;
        this.sb.open(String(err?.error ?? 'Não foi possível redefinir a senha'), 'fechar', { duration: 4000 });
      },
    });
  }

  gotoForgotPassword() {
    this.router.navigateByUrl('/auth/forgot-password');
  }

  private clearMismatchError(): void {
    const confirmControl = this.form.controls.confirmPassword;
    const newPassword = this.form.controls.newPassword.value;
    const confirmation = confirmControl.value;

    if (!confirmControl.errors?.['mismatch']) return;
    if (newPassword !== confirmation) return;

    const { mismatch, ...otherErrors } = confirmControl.errors;
    const hasOtherErrors = Object.keys(otherErrors).length > 0;
    confirmControl.setErrors(hasOtherErrors ? otherErrors : null);
  }
}
