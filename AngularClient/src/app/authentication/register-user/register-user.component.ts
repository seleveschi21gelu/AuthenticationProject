import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { UserModel } from 'src/app/models/UserModel';
import { AuthenticationService } from 'src/app/shared/services/authentication.service';

@Component({
  selector: 'app-register-user',
  templateUrl: './register-user.component.html',
  styleUrls: ['./register-user.component.css']
})
export class RegisterUserComponent implements OnInit {
  registerForm: FormGroup;
  constructor(private authService: AuthenticationService) {

  }

  ngOnInit(): void {
    this.registerForm = new FormGroup({
      firstName: new FormControl(''),
      lastName: new FormControl(''),
      email: new FormControl('', [Validators.required, Validators.email]),
      password: new FormControl('', [Validators.required]),
      confirm: new FormControl('')
    });
  }

  public validateControl = (controlName: string) => {
    return this.registerForm.get(controlName).invalid && this.registerForm.get(controlName).touched
  }

  public hasError = (controlName: string, errorName: string) => {
    return this.registerForm.get(controlName).hasError(errorName);
  }

  public registerUser = (registerFormValue) => {
    const formValues = { ...registerFormValue };

    const user: UserModel = {
      firstName: formValues.firstName,
      lastName: formValues.lastName,
      email: formValues.email,
      password: formValues.password,
      confirmPassword: formValues.confirm
    }

    this.authService.registerUser("api/Account/register", user).subscribe({
      next: (_) => console.log("Successful registration"),
      error: (err: HttpErrorResponse) => console.log(err.error.errors)
    })
  }

}
