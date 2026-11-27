import { Component, ElementRef, AfterViewInit, ViewChild, input, output, signal, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-subtask-export-dialog',
  standalone: true,
  styles: [`
    dialog { width: min(900px, 92vw); margin: auto; padding: 20px; border: none; border-radius: 6px; box-shadow: 0 8px 32px rgba(0,0,0,.3); }
    dialog::backdrop { background: rgba(0,0,0,.4); }
    h3 { margin: 0 0 12px; }
    textarea { height: 60vh; font-family: Consolas, monospace; font-size: .85rem; }
    .form-actions { align-items: center; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <dialog #dialog (close)="closed.emit()">
      <h3>Export (Markdown)</h3>
      <textarea #text class="form-control" readonly [value]="content()"></textarea>
      <div class="form-actions">
        <button type="button" class="btn btn-primary" (click)="copyAll()">Copy All</button>
        <button type="button" class="btn" (click)="dialog.close()">Close</button>
        <span role="status">{{ status() }}</span>
      </div>
    </dialog>
  `
})
export class SubtaskExportDialogComponent implements AfterViewInit {
  content = input.required<string>();
  closed = output<void>();

  status = signal('');

  @ViewChild('dialog') private dialog!: ElementRef<HTMLDialogElement>;
  @ViewChild('text') private text!: ElementRef<HTMLTextAreaElement>;

  ngAfterViewInit(): void {
    this.dialog.nativeElement.showModal();
  }

  copyAll(): void {
    navigator.clipboard.writeText(this.content()).then(
      () => this.status.set('Copied to clipboard.'),
      () => {
        this.text.nativeElement.select();
        this.status.set('Copy failed. The text is selected, press Ctrl+C.');
      }
    );
  }
}
