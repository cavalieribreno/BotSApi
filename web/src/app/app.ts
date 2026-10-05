import { Component, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterOutlet } from '@angular/router';

// Single source of truth for the platform name. Change here to rebrand.
export const PLATFORM_NAME = 'BotSaaS';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  constructor() {
    inject(Title).setTitle(`${PLATFORM_NAME} • Agenda Inteligente`);
  }
}
