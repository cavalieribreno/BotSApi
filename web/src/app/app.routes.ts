import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { AppointmentsComponent } from './pages/appointments/appointments.component';
import { ConversationComponent } from './pages/conversation/conversation.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'appointments', component: AppointmentsComponent, canActivate: [authGuard] },
  { path: 'conversations/:id', component: ConversationComponent, canActivate: [authGuard] },
  { path: '', redirectTo: 'appointments', pathMatch: 'full' }
];
