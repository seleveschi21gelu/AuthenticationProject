import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthResponse } from 'src/app/models/authResponse.model';
import { AuthenticationService } from '../services/authentication.service';
import { EnvironmentUrlService } from '../services/environment-url.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authService: AuthenticationService, private router: Router, private http: HttpClient,
    private envUrl: EnvironmentUrlService) { }

  async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    if (this.authService.isUserAuthenticated()) {
      return true;
    }

    const token = localStorage.getItem("token");
    const isRefreshTokenSuccess = await this.tryRefreshingTokens(token)

    if (isRefreshTokenSuccess) {
      return true;
    }

    if (!isRefreshTokenSuccess) {
      this.router.navigate(['/authentication/login'], { queryParams: { returnUrl: state.url } });
    }

    return false;
  }

  private async tryRefreshingTokens(token: string): Promise<boolean> {
    const refreshToken: string = localStorage.getItem("refreshToken");
    if (!token || !refreshToken) {
      return false;
    }

    const credentials = JSON.stringify({ accessToken: token, refreshToken: refreshToken });
    let isRefreshSuccess: boolean;
    const refreshRes = await new Promise<AuthResponse>((resolve, reject) => {
      this.http.post<AuthResponse>(`${this.envUrl.urlAddress}/api/account/refresh`, credentials, {
        headers: new HttpHeaders({
          "Content-Type": "application/json"
        })
      }).subscribe({
        next: (res: AuthResponse) => resolve(res),
        error: (_) => { reject; isRefreshSuccess = false; }
      });
    });
    localStorage.setItem("token", refreshRes.token);
    localStorage.setItem("refreshToken", refreshRes.refreshToken);
    isRefreshSuccess = true;

    return isRefreshSuccess;
  }

}
