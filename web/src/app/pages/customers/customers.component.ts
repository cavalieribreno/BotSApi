import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CustomerService } from '../../services/customer.service';
import { AppointmentService } from '../../services/appointment.service';
import { Customer } from '../../models/customer';
import { Appointment } from '../../models/appointment';

@Component({
  selector: 'app-customers',
  imports: [CommonModule, DatePipe, FormsModule],
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.css']
})
export class CustomersComponent implements OnInit {
  customers = signal<Customer[]>([]);
  loading = signal(true);
  error = signal('');
  successMessage = signal('');
  searchTerm = signal('');

  // Modal de Novo Cliente
  showAddModal = signal(false);
  saving = signal(false);
  newName = signal('');
  newPhone = signal('');
  addError = signal('');

  // Modal de Histórico de Atendimentos
  showHistoryModal = signal(false);
  historyLoading = signal(false);
  selectedCustomer = signal<Customer | null>(null);
  customerAppointments = signal<Appointment[]>([]);

  filteredCustomers = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const list = this.customers();
    if (!term) {
      return list;
    }
    return list.filter(c =>
      (c.name && c.name.toLowerCase().includes(term)) ||
      (c.phone && c.phone.includes(term))
    );
  });

  constructor(
    private customerService: CustomerService,
    private appointmentService: AppointmentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.customerService.getCustomers().subscribe({
      next: (data) => {
        data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        this.customers.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Não foi possível carregar a lista de clientes.');
        this.loading.set(false);
      }
    });
  }

  openAddModal(): void {
    this.newName.set('');
    this.newPhone.set('');
    this.addError.set('');
    this.saving.set(false);
    this.showAddModal.set(true);
  }

  closeAddModal(): void {
    this.showAddModal.set(false);
    this.addError.set('');
  }

  onPhoneInputChange(val: string): void {
    let digits = val.replace(/\D/g, '');
    if (digits.length > 11) {
      digits = digits.substring(0, 11);
    }

    let masked = digits;
    if (digits.length > 2 && digits.length <= 6) {
      masked = `(${digits.substring(0, 2)}) ${digits.substring(2)}`;
    } else if (digits.length > 6 && digits.length <= 10) {
      masked = `(${digits.substring(0, 2)}) ${digits.substring(2, 6)}-${digits.substring(6)}`;
    } else if (digits.length > 10) {
      masked = `(${digits.substring(0, 2)}) ${digits.substring(2, 7)}-${digits.substring(7, 11)}`;
    }

    this.newPhone.set(masked);
  }

  createCustomer(): void {
    const name = this.newName().trim();
    if (!name) {
      this.addError.set('Por favor, informe o nome do cliente.');
      return;
    }

    const rawPhone = this.newPhone().trim();
    const phoneDigits = rawPhone.replace(/\D/g, '');
    if (!phoneDigits) {
      this.addError.set('Por favor, informe o telefone com DDD.');
      return;
    }

    if (phoneDigits.length < 10 || phoneDigits.length > 11) {
      this.addError.set('Por favor, informe um telefone válido com DDD (10 ou 11 dígitos).');
      return;
    }

    this.saving.set(true);
    this.addError.set('');

    this.customerService.createCustomer({ name, phone: phoneDigits }).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeAddModal();
        this.successMessage.set('Cliente cadastrado com sucesso!');
        this.load();
        setTimeout(() => this.successMessage.set(''), 3000);
      },
      error: (err) => {
        this.saving.set(false);
        let msg = 'Erro ao cadastrar cliente.';
        if (err.error && err.error.error) {
          msg = err.error.error;
        }
        this.addError.set(msg);
      }
    });
  }

  openHistoryModal(customer: Customer): void {
    this.selectedCustomer.set(customer);
    this.showHistoryModal.set(true);
    this.historyLoading.set(true);
    this.customerAppointments.set([]);

    this.appointmentService.getAppointmentsByCustomer(customer.phone).subscribe({
      next: (apts) => {
        apts.sort((a, b) => new Date(b.scheduledAt).getTime() - new Date(a.scheduledAt).getTime());
        this.customerAppointments.set(apts);
        this.historyLoading.set(false);
      },
      error: () => {
        this.historyLoading.set(false);
      }
    });
  }

  closeHistoryModal(): void {
    this.showHistoryModal.set(false);
    this.selectedCustomer.set(null);
  }

  getInitials(name: string): string {
    if (!name) return 'C';
    const parts = name.trim().split(' ').filter(p => p.length > 0);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }

  formatPhone(phone: string): string {
    if (!phone) return '';
    const digits = phone.replace(/\D/g, '');
    if (digits.length === 11) {
      return `(${digits.substring(0, 2)}) ${digits.substring(2, 7)}-${digits.substring(7)}`;
    }
    if (digits.length === 10) {
      return `(${digits.substring(0, 2)}) ${digits.substring(2, 6)}-${digits.substring(6)}`;
    }
    return phone;
  }

  getWhatsAppLink(phone: string): string {
    if (!phone) return '#';
    let digits = phone.replace(/\D/g, '');
    if (digits.length === 10 || digits.length === 11) {
      digits = `55${digits}`;
    }
    return `https://wa.me/${digits}`;
  }

  getTelegramLink(phone: string): string {
    if (!phone) return '#';
    let digits = phone.replace(/\D/g, '');
    if (digits.length === 10 || digits.length === 11) {
      digits = `55${digits}`;
    }
    return `https://t.me/+${digits}`;
  }
}
