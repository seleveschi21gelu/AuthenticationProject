import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UserModel } from 'src/app/models/UserModel';
import { RegisterResponse } from 'src/app/models/RegisterResponse';
import { EnvironmentUrlService } from './environment-url.service';
import { LoginModel } from 'src/app/models/loginModel';
import { AuthResponse } from 'src/app/models/authResponse.model';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {
private authChangeSub = new Subject<boolean>();
public authChanged = this.authChangeSub.asObservable();
  constructor(private http: HttpClient, private envUrl: EnvironmentUrlService) { }

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
}
