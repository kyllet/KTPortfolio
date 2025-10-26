import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { AuthService } from '../../services/auth.service';
import { Credentials } from '../../interfaces/auth';
import { Router } from '@angular/router';
@Component({
  selector: 'app-login',
  imports: [
    CardModule,
    InputTextModule,
    ReactiveFormsModule,
    ButtonModule,
    PasswordModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  authService = inject(AuthService);
  router = inject(Router);

  formBuilder = inject(FormBuilder);
  form = this.formBuilder.group({
    email: ['', { validators: [Validators.required, Validators.email] }],
    password: ['', { validators: [Validators.required] }],
  });

  submit() {
    const creds = this.form.value as Credentials;
    console.log('submit');
    this.authService.login(creds).subscribe({
      next: () => {
        this.router.navigate(['/home']);
      },
      error: (error) => {
        console.log(error);
      },
    });
  }
}
