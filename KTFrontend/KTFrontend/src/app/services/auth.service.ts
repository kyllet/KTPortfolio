import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AuthenticationResponse, Credentials } from '../interfaces/auth';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  apiUrl = 'https://localhost:7247/api';

  private http = inject(HttpClient);
  private baseURL = this.apiUrl + '/users';
  private readonly keyToken = 'token';
  private readonly keyExpiration = 'token-expiration';

  login(creds: Credentials): Observable<AuthenticationResponse> {
    return this.http
      .post<AuthenticationResponse>(`${this.baseURL}/login`, creds)
      .pipe(
        tap((authenticationResponse) => this.storeToken(authenticationResponse))
      );
  }

  storeToken(authenticationResponse: AuthenticationResponse) {
    localStorage.setItem(this.keyToken, authenticationResponse.token);
    localStorage.setItem(
      this.keyExpiration,
      authenticationResponse.expiration.toString()
    );
  }
}
