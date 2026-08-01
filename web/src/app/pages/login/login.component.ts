import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

// Login screen: email/password -> AuthService -> redirect to the appointments list.
@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  email = '';
  password = '';
  error = signal('');   // signal: async-set state that the view shows (zoneless needs this)

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit(): void {
    this.error.set('');
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/appointments']),
      error: () => this.error.set('Credenciais inválidas')
    });
  }
}
