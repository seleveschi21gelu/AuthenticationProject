import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UserModel } from 'src/app/models/userModel';
import { RegisterResponse } from 'src/app/models/registerResponse';
import { EnvironmentUrlService } from './environment-url.service';
import { LoginModel } from 'src/app/models/loginModel';
import { AuthResponse } from 'src/app/models/authResponse.model';
import { Subject } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';
import { ForgotPassword } from 'src/app/models/forgotPassword.model';
import { ResetPassword } from 'src/app/models/resetPassword.model';
import { CustomEncoder } from 'src/app/models/custom-encoder';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {
  private authChangeSub = new Subject<boolean>();
  public authChanged = this.authChangeSub.asObservable();
  constructor(private http: HttpClient, private envUrl: EnvironmentUrlService, private jwtHelper: JwtHelperService) { }

  public registerUser = (route: string, body: UserModel) => {
    return this.http.post<RegisterResponse>(this.createCompleteRoute(route, this.envUrl.urlAddress), body);
  }

  private createCompleteRoute = (route: string, envAddress: string) => {
    return `${envAddress}/${route}`;
  }

  public loginUser = (route: string, body: LoginModel) => {
    return this.http.post<AuthResponse>(this.createCompleteRoute(route, this.envUrl.urlAddress), body);
  }

  public sendAuthStateChangeNotification = (isAuthenticated: boolean) => {
    this.authChangeSub.next(isAuthenticated);
  }

  public logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    this.sendAuthStateChangeNotification(false);
  }

  public isUserAuthenticated = (): boolean => {
    const token = localStorage.getItem("token");

    return token && !this.jwtHelper.isTokenExpired(token);
  }

  public isUserAdmin = (): boolean => {
    const token = localStorage.getItem("token");
    const decodedToken = this.jwtHelper.decodeToken(token);
    const role = decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']

    return role === 'Administrator';
  }

  public forgotPassword = (route: string, body: ForgotPassword) => {
    return this.http.post(this.createCompleteRoute(route, this.envUrl.urlAddress), body);
  }

  public resetPassword = (route: string, body: ResetPassword) => {
    return this.http.post(this.createCompleteRoute(route, this.envUrl.urlAddress), body);
  }

  public confirmEmail = (route: string, token: string, email: string) => {
    let params = new HttpParams({ encoder: new CustomEncoder() });
    params = params.append('token', token);
    params = params.append('email', email);

    return this.http.get(this.createCompleteRoute(route, this.envUrl.urlAddress), { params: params });
  }
}
