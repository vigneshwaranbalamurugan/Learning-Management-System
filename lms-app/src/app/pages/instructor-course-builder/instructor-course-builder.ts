import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { ToastService } from '@services/toast.service';
import { DashboardService } from '@services/dashboard.service';
import { LessonResourcesService } from '@services/lesson-resources.service';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { FormInput } from '@components/form-input/form-input';
import { Button } from '@components/button/button';
import { Loader } from '@components/loader/loader';

@Component({
  selector: 'app-instructor-course-builder',
  standalone: true,
  imports: [CommonModule, FormsModule, ConfirmModal, FormInput, Button, Loader],
  templateUrl: './instructor-course-builder.html'
})
export class InstructorCourseBuilder {
  protected layout = inject(InstructorCourseLayout);
  private toastService = inject(ToastService);
  private dashboardService = inject(DashboardService);
  private resourcesService = inject(LessonResourcesService);
  private router = inject(Router);

  // Section expand/collapse state
  protected expandedSections = signal<Record<number, boolean>>({});

  protected toggleSection(sectionId: number, event: Event) {
    const target = event.target as HTMLElement;
    if (target.closest('button') || target.closest('.cursor-grab') || target.closest('app-button')) {
      return;
    }
    this.expandedSections.update(expanded => ({
      ...expanded,
      [sectionId]: !this.isSectionExpanded(sectionId)
    }));
  }

  protected isSectionExpanded(sectionId: number): boolean {
    const expanded = this.expandedSections();
    return expanded[sectionId] !== false;
  }

  // Lesson expand/collapse state
  protected expandedLessons = signal<Record<number, boolean>>({});

  protected toggleLesson(lessonId: number, event: Event) {
    event.stopPropagation();
    
    const isExpanded = !this.isLessonExpanded(lessonId);
    this.expandedLessons.update(expanded => ({
      ...expanded,
      [lessonId]: isExpanded
    }));

    // If expanding, fetch resources manually as fallback if empty
    if (isExpanded) {
      let currentLesson: any = null;
      this.course?.sections?.forEach((s: any) => {
        const found = s.lessons?.find((l: any) => l.id === lessonId);
        if (found) currentLesson = found;
      });
      
      if (currentLesson && (!currentLesson.resources || currentLesson.resources.length === 0)) {
        this.resourcesService.getResourcesByLesson(lessonId).subscribe({
          next: (res) => {
            currentLesson.resources = res || [];
            currentLesson.resources.sort((a: any, b: any) => (a.sortOrder || 0) - (b.sortOrder || 0));
          },
          error: (err) => console.error('Failed to fetch resources for lesson', err)
        });
      }
    }
  }

  protected isLessonExpanded(lessonId: number): boolean {
    return !!this.expandedLessons()[lessonId];
  }

  // Section modal state
  protected showSectionModal = signal(false);
  protected editSectionId: number | null = null;
  protected sectionTitle = '';
  protected sectionDescription = '';
  protected sectionDuration = '01:00:00';
  protected sectionSortOrder = 0;

  // Lesson ADD modal state
  protected showLessonModal = signal(false);
  protected activeSectionId: number | null = null;
  protected lessonTitle = '';
  protected lessonDescription = '';
  protected lessonType = 'Video';
  protected lessonDuration = 10;
  protected lessonContent = '';
  protected lessonContentUrl = '';
  protected selectedFile: File | null = null;
  protected isSavingLesson = signal(false);

  // Lesson DETAIL panel state
  protected showDetailPanel = signal(false);
  protected selectedLesson: any = null;
  protected isLoadingDetail = signal(false);

  // Lesson EDIT modal state
  protected showEditModal = signal(false);
  protected editLessonId: number | null = null;
  protected editTitle = '';
  protected editDescription = '';
  protected editType = 'Video';
  protected editDuration = 10;
  protected editContent = '';
  protected editContentUrl = '';
  protected editSelectedFile: File | null = null;
  protected isSavingEdit = signal(false);

  // Confirmation Modal state
  protected showDeleteModal = false;
  protected deleteType: 'section' | 'lesson' | 'quiz' | 'assignment' | null = null;
  protected idToDelete: number | null = null;

  protected get course() { return this.layout.course(); }

  protected get deleteModalTitle(): string {
    if (this.deleteType === 'section') return 'Delete Section';
    if (this.deleteType === 'lesson') return 'Delete Lesson';
    if (this.deleteType === 'quiz') return 'Delete Quiz';
    if (this.deleteType === 'assignment') return 'Delete Assignment';
    return 'Delete Item';
  }

  protected get deleteModalMessage(): string {
    if (this.deleteType === 'section') {
      return 'Delete this section and all its contents? This action cannot be undone.';
    }
    if (this.deleteType === 'lesson') {
      return 'Delete this lesson permanently? This action cannot be undone.';
    }
    if (this.deleteType === 'quiz') {
      return 'Delete this quiz permanently? This action cannot be undone.';
    }
    if (this.deleteType === 'assignment') {
      return 'Delete this assignment permanently? This action cannot be undone.';
    }
    return 'Are you sure you want to delete this item?';
  }

  protected lessonTypeLabel(type: string | number): string {
    if (type === 'Video' || type === 0 || type === '0') return 'Video';
    if (type === 'Article' || type === 2 || type === '2') return 'Article';
    if (type === 'Pdf' || type === 1 || type === '1') return 'PDF';
    if (type === 'ExternalLink' || type === 3 || type === '3') return 'External Link';
    return String(type);
  }

  protected resolveTypeString(type: string | number): string {
    if (type === 0 || type === '0' || type === 'Video') return 'Video';
    if (type === 1 || type === '1' || type === 'Pdf') return 'Pdf';
    if (type === 2 || type === '2' || type === 'Article') return 'Article';
    if (type === 3 || type === '3' || type === 'ExternalLink') return 'ExternalLink';
    return 'Video';
  }

  // ── Drag & Drop State ───────────────────────────────────────────────────────
  protected draggedSectionIndex: number | null = null;
  protected draggedLessonIndex: number | null = null;
  protected draggedLessonSectionId: number | null = null;
  protected draggedResourceIndex: number | null = null;
  protected draggedResourceLessonId: number | null = null;
  protected isReordering = signal(false);

  // ── Section Reordering ──────────────────────────────────────────────────────
  protected moveSection(index: number, direction: 'up' | 'down') {
    if (!this.course || !this.course.sections) return;
    const newIndex = direction === 'up' ? index - 1 : index + 1;
    if (newIndex < 0 || newIndex >= this.course.sections.length) return;
    
    const temp = this.course.sections[index];
    this.course.sections[index] = this.course.sections[newIndex];
    this.course.sections[newIndex] = temp;
    
    this.saveSectionOrder();
  }

  protected onSectionDragStart(index: number, event: DragEvent) {
    this.draggedSectionIndex = index;
    this.draggedLessonIndex = null;
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  protected onSectionDragOver(index: number, event: DragEvent) {
    event.preventDefault();
    if (this.draggedSectionIndex === null || this.draggedSectionIndex === index) return;
    
    const sections = this.course?.sections;
    if (!sections) return;
    
    const draggedItem = sections[this.draggedSectionIndex];
    sections.splice(this.draggedSectionIndex, 1);
    sections.splice(index, 0, draggedItem);
    this.draggedSectionIndex = index;
  }

  protected onSectionDrop(event: DragEvent) {
    event.preventDefault();
    if (this.draggedSectionIndex !== null) {
      this.saveSectionOrder();
      this.draggedSectionIndex = null;
    }
  }

  protected saveSectionOrder() {
    if (!this.course || !this.course.sections) return;
    this.isReordering.set(true);
    
    const sectionOrders = this.course.sections.map((sec: any, index: number) => ({
      sectionId: sec.id,
      sortOrder: index + 1
    }));

    this.dashboardService.reorderSections(sectionOrders).subscribe({
      next: () => {
        this.isReordering.set(false);
        this.toastService.showSuccess('Section order saved');
        this.layout.loadCourse(this.course!.id); // reload course
      },
      error: (err) => {
        this.isReordering.set(false);
        this.toastService.showApiError(err, 'Failed to save section order');
        this.layout.loadCourse(this.course!.id); // revert
      }
    });
  }

  // ── Resource Reordering ───────────────────────────────────────────────────────
  
  protected moveResource(lesson: any, resourceIndex: number, direction: 'up' | 'down') {
    if (!lesson || !lesson.resources) return;
    
    const newIndex = direction === 'up' ? resourceIndex - 1 : resourceIndex + 1;
    if (newIndex < 0 || newIndex >= lesson.resources.length) return;
    
    const temp = lesson.resources[resourceIndex];
    lesson.resources[resourceIndex] = lesson.resources[newIndex];
    lesson.resources[newIndex] = temp;
    
    this.saveResourceOrder(lesson);
  }

  protected onResourceDragStart(lessonId: number, resourceIndex: number, event: DragEvent) {
    this.draggedResourceLessonId = lessonId;
    this.draggedResourceIndex = resourceIndex;
    this.draggedLessonIndex = null;
    this.draggedSectionIndex = null;
    event.stopPropagation();
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  protected onResourceDragOver(lessonId: number, resourceIndex: number, event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    
    if (this.draggedResourceLessonId !== lessonId) return; // Disallow cross-lesson drag
    if (this.draggedResourceIndex === null || this.draggedResourceIndex === resourceIndex) return;

    let lesson: any = null;
    this.course?.sections?.forEach((s: any) => {
      const found = s.lessons?.find((l: any) => l.id === lessonId);
      if (found) lesson = found;
    });
    if (!lesson || !lesson.resources) return;
    
    const draggedItem = lesson.resources[this.draggedResourceIndex];
    lesson.resources.splice(this.draggedResourceIndex, 1);
    lesson.resources.splice(resourceIndex, 0, draggedItem);
    this.draggedResourceIndex = resourceIndex;
  }

  protected onResourceDrop(lessonId: number, event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    if (this.draggedResourceIndex !== null && this.draggedResourceLessonId === lessonId) {
      let lesson: any = null;
      this.course?.sections?.forEach((s: any) => {
        const found = s.lessons?.find((l: any) => l.id === lessonId);
        if (found) lesson = found;
      });
      if (lesson) {
        this.saveResourceOrder(lesson);
      }
      this.draggedResourceIndex = null;
      this.draggedResourceLessonId = null;
    }
  }

  protected saveResourceOrder(lesson: any) {
    if (!lesson || !lesson.resources) return;
    this.isReordering.set(true);
    
    const resourceOrders = lesson.resources.map((res: any, index: number) => ({
      resourceId: res.id,
      sortOrder: index + 1
    }));

    this.resourcesService.reorderResources(lesson.id, { resources: resourceOrders }).subscribe({
      next: () => {
        this.isReordering.set(false);
        this.toastService.showSuccess('Resource order saved');
        this.layout.loadCourse(this.course!.id); // reload course
      },
      error: (err) => {
        this.isReordering.set(false);
        this.toastService.showApiError(err, 'Failed to save resource order');
        this.layout.loadCourse(this.course!.id); // revert
      }
    });
  }

  protected confirmDeleteResource(id: number, event: Event) {
    event.stopPropagation();
    this.deleteType = 'resource' as any;
    this.idToDelete = id;
    this.showDeleteModal = true;
  }

  // ── Lesson Reordering ───────────────────────────────────────────────────────
  
  protected moveLesson(sectionIndex: number, lessonIndex: number, direction: 'up' | 'down') {
    if (!this.course || !this.course.sections) return;
    const section = this.course.sections[sectionIndex];
    if (!section || !section.lessons) return;
    
    const newIndex = direction === 'up' ? lessonIndex - 1 : lessonIndex + 1;
    if (newIndex < 0 || newIndex >= section.lessons.length) return;
    
    const temp = section.lessons[lessonIndex];
    section.lessons[lessonIndex] = section.lessons[newIndex];
    section.lessons[newIndex] = temp;
    
    this.saveLessonOrder(section);
  }

  protected onLessonDragStart(sectionId: number, lessonIndex: number, event: DragEvent) {
    this.draggedLessonSectionId = sectionId;
    this.draggedLessonIndex = lessonIndex;
    this.draggedSectionIndex = null;
    event.stopPropagation();
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  protected onLessonDragOver(sectionId: number, lessonIndex: number, event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    
    if (this.draggedLessonSectionId !== sectionId) return; // Disallow cross-section drag
    if (this.draggedLessonIndex === null || this.draggedLessonIndex === lessonIndex) return;

    const section = this.course?.sections?.find((s: any) => s.id === sectionId);
    if (!section || !section.lessons) return;
    
    const draggedItem = section.lessons[this.draggedLessonIndex];
    section.lessons.splice(this.draggedLessonIndex, 1);
    section.lessons.splice(lessonIndex, 0, draggedItem);
    this.draggedLessonIndex = lessonIndex;
  }

  protected onLessonDrop(sectionId: number, event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    if (this.draggedLessonIndex !== null && this.draggedLessonSectionId === sectionId) {
      const section = this.course?.sections?.find((s: any) => s.id === sectionId);
      if (section) {
        this.saveLessonOrder(section);
      }
      this.draggedLessonIndex = null;
      this.draggedLessonSectionId = null;
    }
  }

  protected saveLessonOrder(section: any) {
    if (!section || !section.lessons) return;
    this.isReordering.set(true);
    
    const lessonOrders = section.lessons.map((lesson: any, index: number) => ({
      lessonId: lesson.id,
      sortOrder: index + 1
    }));

    this.dashboardService.reorderLessons(lessonOrders).subscribe({
      next: () => {
        this.isReordering.set(false);
        this.toastService.showSuccess('Lesson order saved');
      },
      error: (err) => {
        this.isReordering.set(false);
        this.toastService.showApiError(err, 'Failed to save lesson order');
        if (this.course) this.layout.loadCourse(this.course.id); // revert
      }
    });
  }

  // ── Section operations ────────────────────────────────────────────────────

  protected openAddSection() {
    this.editSectionId = null;
    this.sectionTitle = '';
    this.sectionDescription = '';
    this.sectionDuration = '01:00:00';
    this.sectionSortOrder = (this.course?.sections?.length || 0) + 1;
    this.showSectionModal.set(true);
  }

  protected openEditSection(section: any, event: Event) {
    event.stopPropagation();
    this.editSectionId = section.id;
    this.sectionTitle = section.title;
    this.sectionDescription = section.description || '';
    this.sectionDuration = section.estimatedDuration || '01:00:00';
    this.sectionSortOrder = section.sortOrder || 0;
    this.showSectionModal.set(true);
  }

  protected closeSectionModal() {
    this.showSectionModal.set(false);
    this.editSectionId = null;
  }

  protected saveSection() {
    if (!this.course) return;
    if (!this.sectionTitle.trim()) {
      this.toastService.showError('Section title is required.');
      return;
    }
    const data = {
      courseId: this.course.id,
      title: this.sectionTitle,
      description: this.sectionDescription,
      estimatedDuration: this.sectionDuration,
    };
    
    if (this.editSectionId) {
      this.dashboardService.updateSection(this.editSectionId, data).subscribe({
        next: () => {
          this.toastService.showSuccess('Section updated successfully.');
          this.layout.loadCourse(this.course!.id);
          this.closeSectionModal();
        },
        error: (err) => this.toastService.showApiError(err, 'Failed to update section.')
      });
    } else {
      this.dashboardService.createSection(data).subscribe({
        next: () => {
          this.toastService.showSuccess('Section created successfully.');
          this.layout.loadCourse(this.course!.id);
          this.closeSectionModal();
        },
        error: (err) => this.toastService.showApiError(err, 'Failed to create section.')
      });
    }
  }

  protected confirmDeleteSection(sectionId: number, event: Event) {
    event.stopPropagation();
    this.deleteType = 'section';
    this.idToDelete = sectionId;
    this.showDeleteModal = true;
  }

  // ── Lesson Navigation ─────────────────────────────────────────────────────

  protected openAddLesson(sectionId: number) {
    if (!this.course) return;
    this.router.navigate(['/instructor/courses', this.course.slug, 'sections', sectionId, 'lessons', 'new']);
  }

  protected openLessonDetail(lesson: any) {
    if (!this.course) return;
    this.router.navigate(['/instructor/courses', this.course.slug, 'lessons', lesson.id, 'detail']);
  }

  protected openEditLesson(lesson: any, event?: Event) {
    event?.stopPropagation();
    if (!this.course) return;
    this.router.navigate(['/instructor/courses', this.course.slug, 'lessons', lesson.id, 'edit']);
  }

  // ── Lesson DELETE ─────────────────────────────────────────────────────────

  protected confirmDeleteLesson(lessonId: number, event?: Event) {
    event?.stopPropagation();
    this.deleteType = 'lesson';
    this.idToDelete = lessonId;
    this.showDeleteModal = true;
  }

  protected deleteItem() {
    if (!this.course || this.idToDelete === null || !this.deleteType) return;
    if (this.deleteType === 'section') {
      this.dashboardService.deleteSection(this.idToDelete).subscribe({
        next: () => {
          this.toastService.showSuccess('Section deleted successfully.');
          this.layout.loadCourse(this.course!.id);
          this.closeDeleteModal();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to delete section.');
          this.closeDeleteModal();
        }
      });
    } else if (this.deleteType === 'lesson') {
      this.dashboardService.deleteLesson(this.idToDelete).subscribe({
        next: () => {
          this.toastService.showSuccess('Lesson deleted successfully.');
          this.layout.loadCourse(this.course!.id);
          this.closeDeleteModal();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to delete lesson.');
          this.closeDeleteModal();
        }
      });
    } else if (this.deleteType === 'quiz') {
      this.dashboardService.deleteQuiz(this.idToDelete).subscribe({
        next: () => {
          this.toastService.showSuccess('Quiz deleted successfully.');
          this.layout.loadCourse(this.course!.id);
          this.closeDeleteModal();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to delete quiz.');
          this.closeDeleteModal();
        }
      });
    } else if (this.deleteType === 'assignment') {
      this.dashboardService.deleteAssignment(this.idToDelete).subscribe({
        next: () => {
          this.toastService.showSuccess('Assignment deleted successfully.');
          this.layout.loadCourse(this.course!.id);
          this.closeDeleteModal();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to delete assignment.');
          this.closeDeleteModal();
        }
      });
    } else if (this.deleteType === 'resource' as any) {
      this.resourcesService.deleteResource(this.idToDelete).subscribe({
        next: () => {
          this.toastService.showSuccess('Resource deleted successfully.');
          this.layout.loadCourse(this.course!.id);
          this.closeDeleteModal();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to delete resource.');
          this.closeDeleteModal();
        }
      });
    }
  }

  // ── Quiz & Assignment Operations ───────────────────────────────────────────

  protected openEditQuiz(quizId: number, event?: Event) {
    event?.stopPropagation();
    if (!this.course) return;
    this.router.navigate(['/instructor/courses', this.course.slug, 'quizzes', quizId, 'questions']);
  }

  protected confirmDeleteQuiz(quizId: number, event?: Event) {
    event?.stopPropagation();
    this.deleteType = 'quiz';
    this.idToDelete = quizId;
    this.showDeleteModal = true;
  }

  protected openEditAssignment(assignmentId: number, event?: Event) {
    event?.stopPropagation();
    if (!this.course) return;
    this.router.navigate(['/instructor/courses', this.course.slug, 'assignments', assignmentId, 'edit']);
  }

  protected confirmDeleteAssignment(assignmentId: number, event?: Event) {
    event?.stopPropagation();
    this.deleteType = 'assignment';
    this.idToDelete = assignmentId;
    this.showDeleteModal = true;
  }

  protected openAddResource(lessonId: number) {
    if (!this.course) return;
    this.router.navigate(['/instructor/courses', this.course.slug, 'lessons', lessonId, 'resources', 'new']);
  }

  protected openEditResource(resource: any) {
    if (!this.course) return;
    this.router.navigate(['/instructor/courses', this.course.slug, 'resources', resource.id, 'edit']);
  }

  // ── Quiz Navigation ─────────────────────────────────────────────────────────

  protected moveQuiz(sectionIndex: number, quizIndex: number, direction: 'up' | 'down') {
    if (!this.course || !this.course.sections) return;
    const section = this.course.sections[sectionIndex];
    if (!section || !section.quizzes) return;
    
    const newIndex = direction === 'up' ? quizIndex - 1 : quizIndex + 1;
    if (newIndex < 0 || newIndex >= section.quizzes.length) return;
    
    const temp = section.quizzes[quizIndex];
    section.quizzes[quizIndex] = section.quizzes[newIndex];
    section.quizzes[newIndex] = temp;
    
    this.saveQuizOrder(section);
  }

  protected draggedQuizIndex: number | null = null;
  protected draggedQuizSectionId: number | null = null;

  protected onQuizDragStart(sectionId: number, quizIndex: number, event: DragEvent) {
    this.draggedQuizSectionId = sectionId;
    this.draggedQuizIndex = quizIndex;
    this.draggedSectionIndex = null;
    event.stopPropagation();
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  protected onQuizDragOver(sectionId: number, quizIndex: number, event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    
    if (this.draggedQuizSectionId !== sectionId) return; // Disallow cross-section drag
    if (this.draggedQuizIndex === null || this.draggedQuizIndex === quizIndex) return;

    const section = this.course?.sections?.find((s: any) => s.id === sectionId);
    if (!section || !section.quizzes) return;
    
    const draggedItem = section.quizzes[this.draggedQuizIndex];
    section.quizzes.splice(this.draggedQuizIndex, 1);
    section.quizzes.splice(quizIndex, 0, draggedItem);
    this.draggedQuizIndex = quizIndex;
  }

  protected onQuizDrop(sectionId: number, event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    if (this.draggedQuizIndex !== null && this.draggedQuizSectionId === sectionId) {
      const section = this.course?.sections?.find((s: any) => s.id === sectionId);
      if (section) {
        this.saveQuizOrder(section);
      }
      this.draggedQuizIndex = null;
      this.draggedQuizSectionId = null;
    }
  }

  protected saveQuizOrder(section: any) {
    if (!section || !section.quizzes) return;
    this.isReordering.set(true);
    
    const quizOrders = section.quizzes.map((quiz: any, index: number) => ({
      quizId: quiz.id,
      sortOrder: index + 1
    }));

    this.dashboardService.reorderQuizzes(quizOrders).subscribe({
      next: () => {
        this.isReordering.set(false);
        this.toastService.showSuccess('Quiz order saved');
        if (this.course) this.layout.loadCourse(this.course.id);
      },
      error: (err) => {
        this.isReordering.set(false);
        this.toastService.showApiError(err, 'Failed to save quiz order');
        if (this.course) this.layout.loadCourse(this.course.id); // revert
      }
    });
  }

  // ── Assignment Reordering ──────────────────────────────────────────────────

  protected moveAssignment(sectionIndex: number, assignmentIndex: number, direction: 'up' | 'down') {
    if (!this.course || !this.course.sections) return;
    const section = this.course.sections[sectionIndex];
    if (!section || !section.assignments) return;
    
    const newIndex = direction === 'up' ? assignmentIndex - 1 : assignmentIndex + 1;
    if (newIndex < 0 || newIndex >= section.assignments.length) return;
    
    const temp = section.assignments[assignmentIndex];
    section.assignments[assignmentIndex] = section.assignments[newIndex];
    section.assignments[newIndex] = temp;
    
    this.saveAssignmentOrder(section);
  }

  protected draggedAssignmentIndex: number | null = null;
  protected draggedAssignmentSectionId: number | null = null;

  protected onAssignmentDragStart(sectionId: number, assignmentIndex: number, event: DragEvent) {
    this.draggedAssignmentSectionId = sectionId;
    this.draggedAssignmentIndex = assignmentIndex;
    this.draggedSectionIndex = null;
    event.stopPropagation();
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  protected onAssignmentDragOver(sectionId: number, assignmentIndex: number, event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    
    if (this.draggedAssignmentSectionId !== sectionId) return; // Disallow cross-section drag
    if (this.draggedAssignmentIndex === null || this.draggedAssignmentIndex === assignmentIndex) return;

    const section = this.course?.sections?.find((s: any) => s.id === sectionId);
    if (!section || !section.assignments) return;
    
    const draggedItem = section.assignments[this.draggedAssignmentIndex];
    section.assignments.splice(this.draggedAssignmentIndex, 1);
    section.assignments.splice(assignmentIndex, 0, draggedItem);
    this.draggedAssignmentIndex = assignmentIndex;
  }

  protected onAssignmentDrop(sectionId: number, event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    if (this.draggedAssignmentIndex !== null && this.draggedAssignmentSectionId === sectionId) {
      const section = this.course?.sections?.find((s: any) => s.id === sectionId);
      if (section) {
        this.saveAssignmentOrder(section);
      }
      this.draggedAssignmentIndex = null;
      this.draggedAssignmentSectionId = null;
    }
  }

  protected saveAssignmentOrder(section: any) {
    if (!section || !section.assignments) return;
    this.isReordering.set(true);

    const assignmentOrders = section.assignments.map((assignment: any, index: number) => ({
      assignmentId: assignment.id,
      sortOrder: index + 1
    }));

    this.dashboardService.reorderAssignments(assignmentOrders).subscribe({
      next: () => {
        this.isReordering.set(false);
        this.toastService.showSuccess('Assignment order saved');
        if (this.course) this.layout.loadCourse(this.course.id);
      },
      error: (err) => {
        this.isReordering.set(false);
        this.toastService.showApiError(err, 'Failed to save assignment order');
        if (this.course) this.layout.loadCourse(this.course.id); // revert
      }
    });
  }

  protected closeDeleteModal() {
    this.showDeleteModal = false;
    this.deleteType = null;
    this.idToDelete = null;
  }
}
