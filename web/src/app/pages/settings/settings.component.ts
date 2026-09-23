import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BusinessHoursService } from '../../services/business-hours.service';
import { BusinessHours } from '../../models/business-hours';

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

// Brazilian display order: Monday to Sunday
const DISPLAY_ORDER = [1, 2, 3, 4, 5, 6, 0];

@Component({
  selector: 'app-settings',
  imports: [FormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
  days = signal<DayDisplay[]>([]);
  loading = signal(true);
  saving = signal(false);
  error = signal('');
  success = signal('');

  constructor(private service: BusinessHoursService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.service.getBusinessHours().subscribe({
      next: (data) => {
        const map = new Map<number, BusinessHours>();
        data.forEach(h => map.set(h.dayOfWeek, h));

        const orderedDays: DayDisplay[] = DISPLAY_ORDER.map(d => {
          const item = map.get(d);
          return {
            dayOfWeek: d,
            name: DAY_NAMES[d],
            opensAt: item ? item.opensAt : '09:00',
            closesAt: item ? item.closesAt : '19:00',
            isClosed: item ? item.isClosed : (d === 0)
          };
        });

        this.days.set(orderedDays);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Não foi possível carregar os horários de funcionamento.');
        this.loading.set(false);
      }
    });
  }

  // Quick action to copy Monday's hours to remaining weekdays
  copyMondayToWeekdays(): void {
    const monday = this.days().find(d => d.dayOfWeek === 1);
    if (!monday) return;

    this.days.update(list => list.map(d => {
      // Apply to Tuesday (2) through Friday (5)
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

  save(): void {
    this.error.set('');
    this.success.set('');

    // Client-side validation: ensure opensAt < closesAt when open
    for (const d of this.days()) {
      if (!d.isClosed && d.opensAt >= d.closesAt) {
        this.error.set(`Em ${d.name}, o horário de abertura deve ser anterior ao de fechamento.`);
        return;
      }
    }

    this.saving.set(true);

    const payload: BusinessHours[] = this.days().map(d => ({
      dayOfWeek: d.dayOfWeek,
      opensAt: d.opensAt,
      closesAt: d.closesAt,
      isClosed: d.isClosed
    }));

    this.service.updateBusinessHours(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.success.set('Horários de funcionamento atualizados com sucesso!');
        setTimeout(() => this.success.set(''), 4000);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err?.error?.error || 'Erro ao salvar horários de funcionamento.');
      }
    });
  }
}
