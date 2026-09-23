import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfessionalService } from '../../services/professional.service';
import { Professional, ProfessionalStatus } from '../../models/professional';

@Component({
  selector: 'app-team',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './team.component.html',
  styleUrls: ['./team.component.css']
})
export class TeamComponent implements OnInit {
  professionals = signal<Professional[]>([]);
  loading = signal(true);
  saving = signal(false);
  error = signal('');
  successMessage = signal('');

  // Add modal state
  showAddModal = signal(false);
  newName = signal('');

  // Edit state
  editingId = signal<string | null>(null);
  editName = signal('');
  editStatus = signal<ProfessionalStatus>('Active');

  constructor(private professionalService: ProfessionalService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.professionalService.getProfessionals().subscribe({
      next: data => {
        this.professionals.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Não foi possível carregar a lista de profissionais.');
        this.loading.set(false);
      }
    });
  }

  openAddModal(): void {
    this.newName.set('');
    this.error.set('');
    this.successMessage.set('');
    this.showAddModal.set(true);
  }

  closeAddModal(): void {
    this.showAddModal.set(false);
    this.newName.set('');
  }

  createProfessional(): void {
    const name = this.newName().trim();
    if (!name) {
      this.error.set('Informe o nome do profissional.');
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.professionalService.createProfessional({ name }).subscribe({
      next: created => {
        this.saving.set(false);
        this.closeAddModal();
        this.successMessage.set(`Profissional ${created.name} adicionado com sucesso!`);
        this.load();
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err.error?.error || 'Erro ao cadastrar profissional.');
      }
    });
  }

  startEdit(p: Professional): void {
    this.editingId.set(p.id);
    this.editName.set(p.name);
    this.editStatus.set(p.status);
    this.error.set('');
    this.successMessage.set('');
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(p: Professional): void {
    const name = this.editName().trim();
    if (!name) {
      this.error.set('O nome não pode ficar vazio.');
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.professionalService.updateProfessional(p.id, {
      name,
      status: this.editStatus()
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.editingId.set(null);
        this.successMessage.set('Profissional atualizado com sucesso!');
        this.load();
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err.error?.error || 'Erro ao atualizar profissional.');
      }
    });
  }

  toggleQuickStatus(p: Professional): void {
    const nextStatus: ProfessionalStatus = p.status === 'Active' ? 'Inactive' : 'Active';
    this.saving.set(true);
    this.professionalService.updateProfessional(p.id, {
      name: p.name,
      status: nextStatus
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.load();
      },
      error: () => {
        this.saving.set(false);
        this.error.set('Não foi possível alterar o status.');
      }
    });
  }

  getInitials(name: string): string {
    if (!name) return 'P';
    const parts = name.trim().split(' ');
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }
}
