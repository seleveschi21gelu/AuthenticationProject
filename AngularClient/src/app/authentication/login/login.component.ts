import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthResponse } from 'src/app/models/authResponse.model';
import { LoginModel } from 'src/app/models/loginModel';
import { AuthenticationService } from 'src/app/shared/services/authentication.service';
import { EnvironmentUrlService } from 'src/app/shared/services/environment-url.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  private returnUrl: string;
  loginForm: FormGroup;
  errorMessage: string = '';
  showError: boolean;

  constructor(private authService: AuthenticationService, private router: Router, private route: ActivatedRoute, 
    private envUrl: EnvironmentUrlService) { }

  ngOnInit(): void {
    this.loginForm = new FormGroup({
      username: new FormControl("", [Validators.required]),
      password: new FormControl("", [Validators.required])
    })
    this.returnUrl = this.route.snapshot.queryParams[''] || '/';
  }

  validateControl = (controlName: string) => {
    return this.loginForm.get(controlName).invalid && this.loginForm.get(controlName).touched;
  }

  hasError = (controlName: string, errorName: string) => {
    return this.loginForm.get(controlName).invalid && this.loginForm.get(controlName).touched;
  }

  loginUser = (loginFormValue) => {
    this.showError = false;
    const login = { ...loginFormValue };

    const authUser: LoginModel = {
      email: login.username,
      password: login.password,
      clientURI: `${this.envUrl.clientUrlAddress}/authentication/forgotPassword`
    }

    this.authService.loginUser('api/account/login', authUser)
      .subscribe({
        next: (res: AuthResponse) => {
          localStorage.setItem("token", res.token);
          localStorage.setItem("refreshToken", res.refreshToken);
          this.authService.sendAuthStateChangeNotification(res.isAuthSuccessful);
          this.router.navigate([this.returnUrl]);
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = err.message;
          this.showError = true;
        }
      })
  }

}
