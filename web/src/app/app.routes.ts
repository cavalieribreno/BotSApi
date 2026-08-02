import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { LayoutComponent } from './layout/layout.component';
import { AppointmentsComponent } from './pages/appointments/appointments.component';
import { ConversationComponent } from './pages/conversation/conversation.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    // Authenticated area: all pages render inside the shell (sidebar + topbar).
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'appointments', component: AppointmentsComponent },
      { path: 'conversations/:id', component: ConversationComponent },
      { path: '', redirectTo: 'appointments', pathMatch: 'full' }
    ]
  }
];
