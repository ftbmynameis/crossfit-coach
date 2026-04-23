import { Component, inject, OnInit, signal, computed, ViewChild } from '@angular/core';
import { MatTableModule, MatTable } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTableDataSource } from '@angular/material/table';
import { ExerciseService } from '../exercise.service';
import { Exercise } from '../exercise.model';

@Component({
  selector: 'app-exercise-list',
  imports: [
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatCheckboxModule,
  ],
  templateUrl: './exercise-list.component.html',
  styleUrl: './exercise-list.component.scss',
})
export class ExerciseListComponent implements OnInit {
  private exerciseService = inject(ExerciseService);

  displayedColumns = ['name', 'mechanic', 'equipment', 'isCrossFit'];
  dataSource = new MatTableDataSource<Exercise>();
  loading = signal(true);
  error = signal<string | null>(null);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
    this.exerciseService.getAll().subscribe({
      next: (exercises) => {
        this.dataSource.data = exercises;
        this.dataSource.sort = this.sort;
        this.dataSource.paginator = this.paginator;
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load exercises.');
        this.loading.set(false);
      },
    });
  }

  applyFilter(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.dataSource.filter = value.trim().toLowerCase();
    this.dataSource.paginator?.firstPage();
  }
}
