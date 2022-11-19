import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RegisterUserComponent } from './register-user/register-user.component';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LoginComponent } from './login/login.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { EmailConfirmationComponent } from './email-confirmation/email-confirmation.component';


@NgModule({
  declarations: [
    RegisterUserComponent,
    LoginComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    EmailConfirmationComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      { path: 'register', component: RegisterUserComponent },
      { path: 'login', component: LoginComponent },
      { path: 'forgotPassword', component: ForgotPasswordComponent },
      { path: 'resetPassword', component: ResetPasswordComponent },
      { path: 'emailConfirmation', component: EmailConfirmationComponent },
    ])
  ]
})
export class AuthenticationModule { }
