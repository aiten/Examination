import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Subtask } from '../models/subtask.model';
import { SubtaskService } from '../services/subtask.service';
import { ExamService } from '../services/exam.service';

@Component({
  selector: 'app-subtask-list',
  standalone: true,
  imports: [FormsModule, RouterModule],
  styles: [`
    .grid-input { width: 100%; padding: 4px 8px; border: 1px solid #ccc; border-radius: 4px; font-size: .95rem; box-sizing: border-box; }
    .grid-input:focus { outline: none; border-color: #1a73e8; box-shadow: 0 0 0 2px rgba(26,115,232,.2); }
    .col-seq { width: 90px; }
    .col-points { width: 110px; }
    .col-bonus { width: 80px; text-align: center; }
    .col-actions { width: 220px; white-space: nowrap; }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>Subtasks{{ examDescription() ? ' — ' + examDescription() : '' }}</h2>
        <a routerLink="/exams" class="btn">Back to Exams</a>
      </div>

      @if (loading()) {
        <p class="empty">Loading...</p>
      }

      <table class="table">
        <thead>
          <tr>
            <th class="col-seq">Seq</th>
            <th>Description</th>
            <th class="col-points">Points</th>
            <th class="col-bonus">Bonus</th>
            <th class="col-actions"></th>
          </tr>
        </thead>
        <tbody>
          @for (s of subtasks(); track s.id) {
            @if (editingId() === s.id) {
              <tr>
                <td class="col-seq">
                  <input type="number" class="grid-input"
                    [ngModel]="editSeqNo()"
                    (ngModelChange)="editSeqNo.set(+$event)"
                    min="1" />
                </td>
                <td>
                  <input class="grid-input"
                    [ngModel]="editDescription()"
                    (ngModelChange)="editDescription.set($event)"
                    placeholder="Description" />
                </td>
                <td class="col-points">
                  <input type="number" class="grid-input"
                    [ngModel]="editPoints()"
                    (ngModelChange)="editPoints.set(+$event)"
                    min="0" />
                </td>
                <td class="col-bonus">
                  <input type="checkbox"
                    [ngModel]="editBonus()"
                    (ngModelChange)="editBonus.set($event)" />
                </td>
                <td class="col-actions">
                  <button class="btn btn-sm btn-primary"
                    (click)="saveEdit(s.id)"
                    [disabled]="!editDescription()">Save</button>
                  <button class="btn btn-sm" (click)="cancelEdit()">Cancel</button>
                </td>
              </tr>
            } @else {
              <tr>
                <td class="col-seq">{{ s.seqNo }}</td>
                <td>{{ s.description }}</td>
                <td class="col-points">{{ s.points }}</td>
                <td class="col-bonus">{{ s.bonus ? 'Yes' : '' }}</td>
                <td class="col-actions">
                  <button class="btn btn-sm" (click)="startEdit(s)">Edit</button>
                  <button class="btn btn-sm btn-danger" (click)="delete(s.id)">Delete</button>
                  <a [routerLink]="['/exams', examId, 'subtasks', s.id, 'students']" class="btn btn-sm">Result</a>
                </td>
              </tr>
            }
          }
          @if (isAdding()) {
            <tr>
              <td class="col-seq">
                <input type="number" class="grid-input"
                  [ngModel]="newSeqNo()"
                  (ngModelChange)="newSeqNo.set(+$event)"
                  min="1" />
              </td>
              <td>
                <input class="grid-input"
                  [ngModel]="newDescription()"
                  (ngModelChange)="newDescription.set($event)"
                  placeholder="Description" />
              </td>
              <td class="col-points">
                <input type="number" class="grid-input"
                  [ngModel]="newPoints()"
                  (ngModelChange)="newPoints.set(+$event)"
                  min="0"
                  placeholder="0" />
              </td>
              <td class="col-bonus">
                <input type="checkbox"
                  [ngModel]="newBonus()"
                  (ngModelChange)="newBonus.set($event)" />
              </td>
              <td class="col-actions">
                <button class="btn btn-sm btn-primary"
                  (click)="addSave()"
                  [disabled]="!newDescription()">Add</button>
                <button class="btn btn-sm" (click)="cancelAdd()">Cancel</button>
              </td>
            </tr>
          }
        </tbody>
      </table>

      @if (!loading() && subtasks().length === 0 && !isAdding()) {
        <p class="empty">No subtasks yet.</p>
      }

      @if (!isAdding()) {
        <div style="margin-top: 12px;">
          <button class="btn btn-primary" (click)="startAdd()">+ Add Subtask</button>
        </div>
      }

      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
    </div>
  `
})
export class SubtaskListComponent implements OnInit {
  examId = 0;

  subtasks = signal<Subtask[]>([]);
  examDescription = signal('');
  loading = signal(false);
  error = signal('');

  editingId = signal<number | null>(null);
  editSeqNo = signal(0);
  editDescription = signal('');
  editPoints = signal(0);
  editBonus = signal(false);

  isAdding = signal(false);
  newSeqNo = signal(0);
  newDescription = signal('');
  newPoints = signal(0);
  newBonus = signal(false);

  constructor(
    private service: SubtaskService,
    private examService: ExamService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.examId = +this.route.snapshot.paramMap.get('examId')!;
    this.examService.getById(this.examId).subscribe(e => this.examDescription.set(e.description));
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.service.getAll(this.examId).subscribe({
      next: data => { this.subtasks.set(data.slice().sort((a, b) => a.seqNo - b.seqNo)); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  startEdit(s: Subtask): void {
    this.editingId.set(s.id);
    this.editSeqNo.set(s.seqNo);
    this.editDescription.set(s.description);
    this.editPoints.set(s.points);
    this.editBonus.set(s.bonus);
    this.isAdding.set(false);
    this.error.set('');
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    const dto: Subtask = { id, seqNo: this.editSeqNo(), description: this.editDescription(), points: this.editPoints(), bonus: this.editBonus() };
    this.service.update(this.examId, id, dto).subscribe({
      next: () => {
        this.subtasks.update(list => list.map(s => s.id === id ? dto : s).slice().sort((a, b) => a.seqNo - b.seqNo));
        this.editingId.set(null);
      },
      error: err => this.error.set(err.error?.detail ?? 'Update failed.')
    });
  }

  delete(id: number): void {
    this.service.delete(this.examId, id).subscribe({
      next: () => this.subtasks.update(list => list.filter(s => s.id !== id)),
      error: err => this.error.set(err.error?.detail ?? 'Delete failed.')
    });
  }

  startAdd(): void {
    const nextSeq = this.subtasks().length > 0
      ? Math.max(...this.subtasks().map(s => s.seqNo)) + 1
      : 1;
    this.isAdding.set(true);
    this.newSeqNo.set(nextSeq);
    this.newDescription.set('');
    this.newPoints.set(0);
    this.newBonus.set(false);
    this.editingId.set(null);
    this.error.set('');
  }

  cancelAdd(): void {
    this.isAdding.set(false);
  }

  addSave(): void {
    const dto: Subtask = { id: 0, seqNo: this.newSeqNo(), description: this.newDescription(), points: this.newPoints(), bonus: this.newBonus() };
    this.service.create(this.examId, dto).subscribe({
      next: created => {
        this.subtasks.update(list => [...list, created].slice().sort((a, b) => a.seqNo - b.seqNo));
        this.isAdding.set(false);
      },
      error: err => this.error.set(err.error?.detail ?? 'Create failed.')
    });
  }
}
