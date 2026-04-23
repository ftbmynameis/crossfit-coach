import { Component, inject, OnInit, signal } from '@angular/core';
import { AgGridAngular } from 'ag-grid-angular';
import {
  ColDef,
  GridReadyEvent,
  GridApi,
  themeQuartz,
} from 'ag-grid-community';
import { ExerciseService } from '../exercise.service';
import { Exercise } from '../exercise.model';

@Component({
  selector: 'app-exercise-list',
  imports: [AgGridAngular],
  templateUrl: './exercise-list.component.html',
  styleUrl: './exercise-list.component.scss',
})
export class ExerciseListComponent implements OnInit {
  private exerciseService = inject(ExerciseService);

  exercises = signal<Exercise[]>([]);
  error = signal<string | null>(null);

  theme = themeQuartz;

  columnDefs: ColDef<Exercise>[] = [
    {
      field: 'name',
      headerName: 'Name',
      flex: 2,
      filter: 'agTextColumnFilter',
      sort: 'asc',
    },
    {
      field: 'mechanic',
      headerName: 'Mechanic',
      flex: 1,
      filter: 'agTextColumnFilter',
      valueFormatter: (p) => p.value ?? '—',
    },
    {
      field: 'equipment',
      headerName: 'Equipment',
      flex: 1,
      filter: 'agTextColumnFilter',
      valueFormatter: (p) => p.value ?? '—',
    },
    {
      field: 'isCrossFit',
      headerName: 'CrossFit',
      width: 110,
      filter: 'agSetColumnFilter',
      cellRenderer: (p: { value: boolean }) =>
        p.value ? '✓' : '',
    },
  ];

  defaultColDef: ColDef = {
    sortable: true,
    resizable: true,
  };

  private gridApi!: GridApi<Exercise>;

  ngOnInit(): void {
    this.exerciseService.getAll().subscribe({
      next: (data) => this.exercises.set(data),
      error: () => this.error.set('Failed to load exercises.'),
    });
  }

  onGridReady(params: GridReadyEvent<Exercise>): void {
    this.gridApi = params.api;
  }

  onFilterInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.gridApi.setGridOption('quickFilterText', value);
  }
}
