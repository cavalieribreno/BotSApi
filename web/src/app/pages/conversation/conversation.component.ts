import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ConversationService } from '../../services/conversation.service';
import { Message } from '../../models/message';

// The chat behind an appointment: loads and shows the conversation's messages in a modern messenger interface.
@Component({
  selector: 'app-conversation',
  imports: [DatePipe, RouterLink],
  templateUrl: './conversation.component.html',
  styleUrls: ['./conversation.component.css']
})
export class ConversationComponent implements OnInit {
  messages = signal<Message[]>([]);
  customerPhone = signal('');
  loading = signal(true);

  constructor(
    private service: ConversationService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.service.getMessages(id).subscribe({
      next: detail => {
        this.messages.set(detail.messages);
        this.customerPhone.set(detail.customerPhone);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  formatPhone(phone: string): string {
    if (!phone) return 'Cliente';
    // Clean and format brazilian numbers if applicable
    const clean = phone.replace(/\D/g, '');
    if (clean.length === 13 && clean.startsWith('55')) {
      return `+55 (${clean.slice(2, 4)}) ${clean.slice(4, 9)}-${clean.slice(9)}`;
    }
    if (clean.length === 11) {
      return `(${clean.slice(0, 2)}) ${clean.slice(2, 7)}-${clean.slice(7)}`;
    }
    return phone;
  }
}
