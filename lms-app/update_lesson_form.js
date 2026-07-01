const fs = require('fs');
const tsPath = 'src/app/pages/instructor-lesson-form/instructor-lesson-form.ts';
const htmlPath = 'src/app/pages/instructor-lesson-form/instructor-lesson-form.html';

let tsContent = fs.readFileSync(tsPath, 'utf8');

// Add signal imports if not present
if (!tsContent.includes('ConfirmModal')) {
    tsContent = tsContent.replace(/import \{.*?\} from '@angular\/core';/, (match) => {
        return match; 
    });
    tsContent = tsContent.replace(/import \{ CourseBuilderService \}/, `import { ConfirmModal } from '@components/confirm-modal/confirm-modal';\nimport { CourseBuilderService }`);
}
// Add ConfirmModal to imports
tsContent = tsContent.replace(/imports: \[([^\]]+)\]/, (match, p1) => {
    if (!p1.includes('ConfirmModal')) {
        return `imports: [${p1}, ConfirmModal]`;
    }
    return match;
});

// Add modal logic
const logicToAdd = `
  protected showUnsavedModal = signal(false);
  private unsavedResolve: ((val: boolean) => void) | null = null;

  async canDeactivate(): Promise<boolean> {
    if (!this.isDirty || this.isSaving()) return true;
    return new Promise<boolean>((resolve) => {
      this.unsavedResolve = resolve;
      this.showUnsavedModal.set(true);
    });
  }

  protected confirmLeave(): void {
    this.showUnsavedModal.set(false);
    if (this.unsavedResolve) {
      this.unsavedResolve(true);
      this.unsavedResolve = null;
    }
  }

  protected cancelLeave(): void {
    this.showUnsavedModal.set(false);
    if (this.unsavedResolve) {
      this.unsavedResolve(false);
      this.unsavedResolve = null;
    }
  }
`;
if (!tsContent.includes('canDeactivate()')) {
    tsContent = tsContent.replace(/protected navigateBack\(\) \{/, logicToAdd + '\n  protected navigateBack() {');
}
fs.writeFileSync(tsPath, tsContent);

let htmlContent = fs.readFileSync(htmlPath, 'utf8');
if (!htmlContent.includes('<app-confirm-modal')) {
    htmlContent += `

<!-- Unsaved Changes Modal -->
<app-confirm-modal 
  [isOpen]="showUnsavedModal()" 
  title="Unsaved Changes"
  message="You have unsaved changes. Are you sure you want to leave this page? All your progress will be lost."
  confirmText="Leave Page"
  cancelText="Stay"
  [isDanger]="true"
  (confirm)="confirmLeave()"
  (cancel)="cancelLeave()">
</app-confirm-modal>
`;
    fs.writeFileSync(htmlPath, htmlContent);
}

