import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// App shell: persistent sidebar + topbar wrapping every authenticated page.
@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layout.component.html'
})
export class LayoutComponent {
  constructor(private auth: AuthService, private router: Router) {}

  get email(): string {
    return this.auth.userEmail ?? 'usuário';
  }

  get initial(): string {
    return this.email.charAt(0).toUpperCase();
  }

  // Tenant brand (white-label): the business name + its initial in the logo slot.
  get company(): string {
    return this.auth.companyName || 'Meu negócio';
  }

  get companyInitial(): string {
    return this.company.charAt(0).toUpperCase();
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
