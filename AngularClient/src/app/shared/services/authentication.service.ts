import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UserModel } from 'src/app/models/UserModel';
import { RegisterResponse } from 'src/app/models/RegisterResponse';
import { EnvironmentUrlService } from './environment-url.service';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {

  constructor(private http: HttpClient, private envUrl: EnvironmentUrlService) { }

  public registerUser = (route: string, body: UserModel) => {
    return this.http.post<RegisterResponse>(this.createCompleteRoute(route, this.envUrl.urlAddress), body);
  }

  private createCompleteRoute = (route: string, envAddress: string) => {
    return `${envAddress}/${route}`;
  }
}
