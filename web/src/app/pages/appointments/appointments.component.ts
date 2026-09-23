import { Component, OnInit, signal, computed } from '@angular/core';
import { DatePipe, CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AppointmentService } from '../../services/appointment.service';
import { ProfessionalService } from '../../services/professional.service';
import { Appointment } from '../../models/appointment';
import { Professional } from '../../models/professional';

// Editorial appointment dashboard matching the luxury barbershop design
@Component({
  selector: 'app-appointments',
  imports: [CommonModule, DatePipe, FormsModule, RouterLink],
  templateUrl: './appointments.component.html',
  styleUrls: ['./appointments.component.css']
})
export class AppointmentsComponent implements OnInit {
  appointments = signal<Appointment[]>([]);
  professionals = signal<Professional[]>([]);
  loading = signal(true);
  error = signal('');
  
  viewMode = signal<'grid' | 'list'>('grid');
  statusFilter = signal<string>('All');
  professionalFilter = signal<string>('All');
  searchTerm = signal<string>('');

  readonly gridHours: string[] = [
    '08:00', '09:00', '10:00', '11:00', '12:00', '13:00', 
    '14:00', '15:00', '16:00', '17:00', '18:00', '19:00'
  ];

  constructor(
    private service: AppointmentService,
    private professionalService: ProfessionalService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');

    // Load active professionals
    this.professionalService.getProfessionals('Active').subscribe({
      next: pros => this.professionals.set(pros),
      error: () => {}
    });

    // Load appointments
    this.service.getAppointments().subscribe({
      next: data => {
        // Sort chronologically
        data.sort((a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime());
        this.appointments.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Não foi possível carregar os agendamentos.');
        this.loading.set(false);
      }
    });
  }

  // Today date breakdown for the editorial block
  get todayDay(): number {
    return new Date().getDate();
  }

  get todayMonth(): string {
    const m = new Date().toLocaleDateString('pt-BR', { month: 'short' });
    return m.replace('.', '');
  }

  get todayWeekday(): string {
    const w = new Date().toLocaleDateString('pt-BR', { weekday: 'long' });
    return w.charAt(0).toUpperCase() + w.slice(1);
  }

  // Controle de Data Selecionada da Agenda
  selectedDate = signal<Date>(new Date());

  selectedDateFormatted = computed(() => {
    const d = this.selectedDate();
    const isTod = this.isToday(d);
    const day = d.getDate().toString().padStart(2, '0');
    const months = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
    const month = months[d.getMonth()];
    const weekdays = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];
    const weekday = weekdays[d.getDay()];
    
    if (isTod) {
      return `Hoje, ${day} ${month} (${weekday})`;
    }
    return `${day} ${month} (${weekday})`;
  });

  isToday(d: Date): boolean {
    const now = new Date();
    return d.getFullYear() === now.getFullYear() &&
           d.getMonth() === now.getMonth() &&
           d.getDate() === now.getDate();
  }

  isSameDay(d1: Date, d2: Date): boolean {
    return d1.getFullYear() === d2.getFullYear() &&
           d1.getMonth() === d2.getMonth() &&
           d1.getDate() === d2.getDate();
  }

  previousDay(): void {
    const d = new Date(this.selectedDate());
    d.setDate(d.getDate() - 1);
    this.selectedDate.set(d);
  }

  nextDay(): void {
    const d = new Date(this.selectedDate());
    d.setDate(d.getDate() + 1);
    this.selectedDate.set(d);
  }

  goToToday(): void {
    this.selectedDate.set(new Date());
  }

  // Agendamentos da data selecionada
  selectedDateAppointments = computed(() => {
    const sel = this.selectedDate();
    return this.appointments().filter(a => {
      if (a.status === 'Cancelled') return false;
      const d = new Date(a.scheduledAt);
      return this.isSameDay(d, sel);
    });
  });

  pendingCount = computed(() => this.selectedDateAppointments().filter(a => a.status === 'Pending').length);
  confirmedCount = computed(() => this.selectedDateAppointments().filter(a => a.status === 'Confirmed').length);
  completedCount = computed(() => this.selectedDateAppointments().filter(a => a.status === 'Completed').length);
  totalCount = computed(() => this.selectedDateAppointments().length);

  // Next active appointment for the dark featured card
  nextAppointment = computed(() => {
    const active = this.selectedDateAppointments().filter(a => a.status === 'Pending' || a.status === 'Confirmed');
    return active.length > 0 ? active[0] : null;
  });

  filteredProfessionals = computed(() => {
    const filter = this.professionalFilter();
    const list = this.professionals();
    if (filter === 'All') return list;
    return list.filter(p => p.id === filter);
  });

  filteredAppointments = computed(() => {
    let list = this.appointments();
    const sel = this.selectedDate();
    
    // Filtra pelo dia selecionado
    list = list.filter(a => {
      const d = new Date(a.scheduledAt);
      return this.isSameDay(d, sel);
    });

    const filter = this.statusFilter();
    if (filter !== 'All') {
      list = list.filter(a => a.status === filter);
    }
    const proFilter = this.professionalFilter();
    if (proFilter !== 'All') {
      list = list.filter(a => a.professionalId === proFilter);
    }
    const search = this.searchTerm().trim().toLowerCase();
    if (search) {
      list = list.filter(a =>
        (a.customerName && a.customerName.toLowerCase().includes(search)) ||
        (a.serviceName && a.serviceName.toLowerCase().includes(search)) ||
        (a.professionalName && a.professionalName.toLowerCase().includes(search))
      );
    }
    return list;
  });

  getAppointmentsForSlot(professionalId: string, hourStr: string): Appointment[] {
    const sel = this.selectedDate();
    return this.appointments().filter(a => {
      if (a.professionalId !== professionalId) return false;
      if (a.status === 'Cancelled') return false;
      const d = new Date(a.scheduledAt);
      if (!this.isSameDay(d, sel)) return false;
      const h = d.getHours().toString().padStart(2, '0') + ':00';
      return h === hourStr;
    });
  }

  changeStatus(id: string, status: string): void {
    this.service.updateStatus(id, status).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Não foi possível atualizar o status. Tente novamente.')
    });
  }

  cancel(id: string): void {
    if (confirm('Deseja realmente cancelar este agendamento?')) {
      this.changeStatus(id, 'Cancelled');
    }
  }

  isProDropdownOpen = signal(false);
  isStatusDropdownOpen = signal(false);

  selectedProfessionalName = computed(() => {
    const id = this.professionalFilter();
    if (id === 'All') return 'Todos os Profissionais';
    const pro = this.professionals().find(p => p.id === id);
    return pro ? pro.name : 'Todos os Profissionais';
  });

  selectedStatusName = computed(() => {
    const status = this.statusFilter();
    switch (status) {
      case 'Pending': return 'Pendentes';
      case 'Confirmed': return 'Confirmados';
      case 'Completed': return 'Concluídos';
      default: return 'Todos os Status';
    }
  });

  setFilter(filter: string): void {
    this.statusFilter.set(filter);
  }

  selectStatus(status: string): void {
    this.statusFilter.set(status);
    this.isStatusDropdownOpen.set(false);
  }

  toggleStatusDropdown(): void {
    this.isStatusDropdownOpen.update(v => !v);
    if (this.isStatusDropdownOpen()) {
      this.isProDropdownOpen.set(false);
    }
  }

  setProfessionalFilter(proId: string): void {
    this.professionalFilter.set(proId);
  }

  selectProfessional(proId: string): void {
    this.professionalFilter.set(proId);
    this.isProDropdownOpen.set(false);
  }

  toggleProDropdown(): void {
    this.isProDropdownOpen.update(v => !v);
    if (this.isProDropdownOpen()) {
      this.isStatusDropdownOpen.set(false);
    }
  }

  setViewMode(mode: 'grid' | 'list'): void {
    this.viewMode.set(mode);
  }

  count(status: string): number {
    return this.appointments().filter(a => a.status === status).length;
  }

  countForProfessional(professionalId: string): number {
    return this.gridHours.reduce((acc, hour) => acc + this.getAppointmentsForSlot(professionalId, hour).length, 0);
  }

  getInitials(name: string): string {
    if (!name) return 'C';
    const parts = name.trim().split(' ').filter(p => p.length > 0);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }

  // Determine period for timeline grouping visual separation
  isFirstInPeriod(index: number): boolean {
    if (index === 0) return true;
    const currentList = this.filteredAppointments();
    const prevHour = new Date(currentList[index - 1].scheduledAt).getHours();
    const curHour = new Date(currentList[index].scheduledAt).getHours();
    
    // Group: Manhã (< 12), Tarde (12-18), Noite (>= 18)
    const prevPeriod = prevHour < 12 ? 'Manhã' : (prevHour < 18 ? 'Tarde' : 'Noite');
    const curPeriod = curHour < 12 ? 'Manhã' : (curHour < 18 ? 'Tarde' : 'Noite');
    return prevPeriod !== curPeriod;
  }

  getPeriodName(dateStr: string): string {
    const hour = new Date(dateStr).getHours();
    if (hour < 12) return 'Manhã';
    if (hour < 18) return 'Tarde';
    return 'Noite';
  }
}
