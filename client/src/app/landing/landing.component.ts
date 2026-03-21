import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { environment } from '../../environments/environment';

interface HealthResponse {
  appName: string;
  version: string;
}

@Component({
  selector: 'app-landing',
  imports: [MatToolbarModule, MatCardModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent implements OnInit {
  private http = inject(HttpClient);

  apiStatus = signal<string>('Checking API…');

  ngOnInit(): void {
    this.http
      .get<HealthResponse>(`${environment.apiBaseUrl}/api/health`)
      .subscribe({
        next: (res) => this.apiStatus.set(`${res.appName} v${res.version} — connected`),
        error: () => this.apiStatus.set('API unreachable'),
      });
  }
}
