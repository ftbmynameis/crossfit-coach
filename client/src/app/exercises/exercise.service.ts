import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Exercise } from './exercise.model';

@Injectable({ providedIn: 'root' })
export class ExerciseService {
  private http = inject(HttpClient);

  getAll(): Observable<Exercise[]> {
    return this.http.get<Exercise[]>(`${environment.apiBaseUrl}/api/exercises`);
  }
}
