import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ConversationService } from '../../services/conversation.service';
import { Message } from '../../models/message';

// The chat behind an appointment: loads and shows the conversation's messages.
@Component({
  selector: 'app-conversation',
  imports: [DatePipe, RouterLink],
  templateUrl: './conversation.component.html'
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
}
