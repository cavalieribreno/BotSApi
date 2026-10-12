import { Component, OnInit, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BusinessHoursService } from '../../services/business-hours.service';
import { BusinessHours } from '../../models/business-hours';
import { ChannelService, ChannelResponse } from '../../core/services/channel.service';
import { ProfessionalService } from '../../services/professional.service';
import { Professional, ProfessionalStatus } from '../../models/professional';

export type SettingsTab = 'channels' | 'team' | 'hours';

interface DayDisplay {
  dayOfWeek: number;
  name: string;
  opensAt: string;
  closesAt: string;
  isClosed: boolean;
}

const DAY_NAMES: Record<number, string> = {
  1: 'Segunda-feira',
  2: 'Terça-feira',
  3: 'Quarta-feira',
  4: 'Quinta-feira',
  5: 'Sexta-feira',
  6: 'Sábado',
  0: 'Domingo'
};

const DISPLAY_ORDER = [1, 2, 3, 4, 5, 6, 0];

@Component({
  selector: 'app-settings',
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
  private readonly hoursService = inject(BusinessHoursService);
  private readonly channelService = inject(ChannelService);
  private readonly professionalService = inject(ProfessionalService);

  // Active Tab
  activeTab = signal<SettingsTab>('channels');

  // Shared Feedback Signals
  error = signal('');
  success = signal('');

  // ----------------------------------------------------
  // Tab 1: Channels (Bots)
  // ----------------------------------------------------
  channels = signal<ChannelResponse[]>([]);
  channelsLoading = signal(false);
  channelsSaving = signal(false);
  showAddChannelModal = signal(false);
  channelModalError = signal('');
  newChannelToken = signal('');

  // ----------------------------------------------------
  // Tab 2: Team (Profissionais)
  // ----------------------------------------------------
  professionals = signal<Professional[]>([]);
  teamLoading = signal(false);
  teamSaving = signal(false);
  showAddTeamModal = signal(false);
  newTeamName = signal('');
  editingTeamId = signal<string | null>(null);
  editTeamName = signal('');
  editTeamStatus = signal<ProfessionalStatus>('Active');

  // ----------------------------------------------------
  // Tab 3: Business Hours (Horários)
  // ----------------------------------------------------
  days = signal<DayDisplay[]>([]);
  hoursLoading = signal(false);
  hoursSaving = signal(false);

  ngOnInit(): void {
    this.loadChannels();
    this.loadTeam();
    this.loadHours();
  }

  setTab(tab: SettingsTab): void {
    this.activeTab.set(tab);
    this.error.set('');
    this.success.set('');
  }

  // ----------------------------------------------------
  // Channels Logic
  // ----------------------------------------------------
  loadChannels(): void {
    this.channelsLoading.set(true);
    this.channelService.getChannels().subscribe({
      next: (data: ChannelResponse[]) => {
        this.channels.set(data);
        this.channelsLoading.set(false);
      },
      error: () => {
        this.error.set('Não foi possível carregar os canais de atendimento.');
        this.channelsLoading.set(false);
      }
    });
  }

  openAddChannelModal(): void {
    this.newChannelToken.set('');
    this.channelModalError.set('');
    this.showAddChannelModal.set(true);
  }

  closeAddChannelModal(): void {
    this.showAddChannelModal.set(false);
    this.channelModalError.set('');
    this.newChannelToken.set('');
  }

  registerChannel(): void {
    const token = this.newChannelToken().trim();
    if (!token) {
      this.channelModalError.set('Por favor, informe o Token do Bot.');
      return;
    }

    this.channelsSaving.set(true);
    this.channelModalError.set('');
    this.error.set('');

    this.channelService.registerChannel({
      type: 1, // Telegram
      identifier: token
    }).subscribe({
      next: (created: ChannelResponse) => {
        this.channels.update(list => [...list, created]);
        this.channelsSaving.set(false);
        this.closeAddChannelModal();
        this.success.set('Bot do Telegram conectado com sucesso!');
        setTimeout(() => this.success.set(''), 4000);
      },
      error: (err: any) => {
        this.channelsSaving.set(false);
        let msg = 'Erro ao conectar canal.';
        if (err && err.error && err.error.error) {
          msg = err.error.error;
        }

        if (msg.includes('Invalid telegram bot token')) {
          msg = 'Token inválido. Verifique o código copiado do @BotFather no Telegram.';
        } else if (msg.includes('APP_BASE_URL')) {
          msg = 'A variável APP_BASE_URL não está configurada no backend para webhooks.';
        }

        this.channelModalError.set(msg);
      }
    });
  }

  deleteChannel(channel: ChannelResponse): void {
    const confirmed = confirm('Tem certeza que deseja desconectar este canal?');
    if (!confirmed) {
      return;
    }

    this.channelsSaving.set(true);
    this.error.set('');

    this.channelService.deleteChannel(channel.id).subscribe({
      next: () => {
        this.channels.update(list => list.filter(c => c.id !== channel.id));
        this.channelsSaving.set(false);
        this.success.set('Canal desconectado com sucesso.');
        setTimeout(() => this.success.set(''), 4000);
      },
      error: (err: any) => {
        this.channelsSaving.set(false);
        let msg = 'Erro ao desconectar canal.';
        if (err && err.error && err.error.error) {
          msg = err.error.error;
        }
        this.error.set(msg);
      }
    });
  }

  getChannelProviderName(type: number): string {
    if (type === 1) {
      return 'Telegram';
    }
    if (type === 0) {
      return 'WhatsApp';
    }
    return 'Canal';
  }

  // ----------------------------------------------------
  // Team Logic
  // ----------------------------------------------------
  loadTeam(): void {
    this.teamLoading.set(true);
    this.professionalService.getProfessionals().subscribe({
      next: (data: Professional[]) => {
        this.professionals.set(data);
        this.teamLoading.set(false);
      },
      error: () => {
        this.error.set('Não foi possível carregar a equipe.');
        this.teamLoading.set(false);
      }
    });
  }

  openAddTeamModal(): void {
    this.newTeamName.set('');
    this.showAddTeamModal.set(true);
  }

  closeAddTeamModal(): void {
    this.showAddTeamModal.set(false);
    this.newTeamName.set('');
  }

  createProfessional(): void {
    const name = this.newTeamName().trim();
    if (!name) {
      return;
    }

    this.teamSaving.set(true);
    this.professionalService.createProfessional({ name }).subscribe({
      next: (created: Professional) => {
        this.professionals.update(list => [...list, created]);
        this.teamSaving.set(false);
        this.closeAddTeamModal();
        this.success.set(`Profissional "${created.name}" cadastrado com sucesso!`);
        setTimeout(() => this.success.set(''), 4000);
      },
      error: (err: any) => {
        this.teamSaving.set(false);
        let msg = 'Erro ao cadastrar profissional.';
        if (err && err.error && err.error.error) {
          msg = err.error.error;
        }
        this.error.set(msg);
      }
    });
  }

  startEditProfessional(p: Professional): void {
    this.editingTeamId.set(p.id);
    this.editTeamName.set(p.name);
    this.editTeamStatus.set(p.status);
  }

  cancelEditProfessional(): void {
    this.editingTeamId.set(null);
  }

  saveEditProfessional(): void {
    const id = this.editingTeamId();
    if (!id) {
      return;
    }

    const name = this.editTeamName().trim();
    if (!name) {
      return;
    }

    this.teamSaving.set(true);
    this.professionalService.updateProfessional(id, {
      name,
      status: this.editTeamStatus()
    }).subscribe({
      next: () => {
        this.professionals.update(list => list.map(item => {
          if (item.id === id) {
            return {
              ...item,
              name: name,
              status: this.editTeamStatus()
            };
          }
          return item;
        }));
        this.editingTeamId.set(null);
        this.teamSaving.set(false);
        this.success.set('Profissional atualizado com sucesso!');
        setTimeout(() => this.success.set(''), 4000);
      },
      error: (err: any) => {
        this.teamSaving.set(false);
        let msg = 'Erro ao atualizar profissional.';
        if (err && err.error && err.error.error) {
          msg = err.error.error;
        }
        this.error.set(msg);
      }
    });
  }

  toggleQuickStatus(p: Professional): void {
    let nextStatus: ProfessionalStatus = 'Inactive';
    if (p.status !== 'Active') {
      nextStatus = 'Active';
    }

    this.teamSaving.set(true);
    this.professionalService.updateProfessional(p.id, {
      name: p.name,
      status: nextStatus
    }).subscribe({
      next: () => {
        this.professionals.update(list => list.map(item => {
          if (item.id === p.id) {
            return {
              ...item,
              status: nextStatus
            };
          }
          return item;
        }));
        this.teamSaving.set(false);
      },
      error: (err: any) => {
        this.teamSaving.set(false);
        let msg = 'Erro ao alterar status.';
        if (err && err.error && err.error.error) {
          msg = err.error.error;
        }
        this.error.set(msg);
      }
    });
  }

  getInitials(name: string): string {
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.slice(0, 2).toUpperCase();
  }

  // ----------------------------------------------------
  // Business Hours Logic
  // ----------------------------------------------------
  loadHours(): void {
    this.hoursLoading.set(true);
    this.hoursService.getBusinessHours().subscribe({
      next: (data: BusinessHours[]) => {
        const map = new Map<number, BusinessHours>();
        data.forEach(h => map.set(h.dayOfWeek, h));

        const orderedDays: DayDisplay[] = DISPLAY_ORDER.map(d => {
          const item = map.get(d);
          let opensAt = '09:00';
          let closesAt = '19:00';
          let isClosed = (d === 0);

          if (item) {
            opensAt = item.opensAt;
            closesAt = item.closesAt;
            isClosed = item.isClosed;
          }

          return {
            dayOfWeek: d,
            name: DAY_NAMES[d],
            opensAt: opensAt,
            closesAt: closesAt,
            isClosed: isClosed
          };
        });

        this.days.set(orderedDays);
        this.hoursLoading.set(false);
      },
      error: () => {
        this.error.set('Não foi possível carregar os horários de funcionamento.');
        this.hoursLoading.set(false);
      }
    });
  }

  copyMondayToWeekdays(): void {
    const monday = this.days().find(d => d.dayOfWeek === 1);
    if (!monday) {
      return;
    }

    this.days.update(list => list.map(d => {
      if (d.dayOfWeek >= 2 && d.dayOfWeek <= 5) {
        return {
          ...d,
          opensAt: monday.opensAt,
          closesAt: monday.closesAt,
          isClosed: monday.isClosed
        };
      }
      return d;
    }));
  }

  saveHours(): void {
    this.error.set('');
    this.success.set('');

    for (const d of this.days()) {
      if (!d.isClosed && d.opensAt >= d.closesAt) {
        this.error.set(`Em ${d.name}, o horário de abertura deve ser anterior ao de fechamento.`);
        return;
      }
    }

    this.hoursSaving.set(true);

    const payload: BusinessHours[] = this.days().map(d => ({
      dayOfWeek: d.dayOfWeek,
      opensAt: d.opensAt,
      closesAt: d.closesAt,
      isClosed: d.isClosed
    }));

    this.hoursService.updateBusinessHours(payload).subscribe({
      next: () => {
        this.hoursSaving.set(false);
        this.success.set('Horários de funcionamento atualizados com sucesso!');
        setTimeout(() => this.success.set(''), 4000);
      },
      error: (err: any) => {
        this.hoursSaving.set(false);
        let msg = 'Erro ao salvar horários de funcionamento.';
        if (err && err.error && err.error.error) {
          msg = err.error.error;
        }
        this.error.set(msg);
      }
    });
  }
}
