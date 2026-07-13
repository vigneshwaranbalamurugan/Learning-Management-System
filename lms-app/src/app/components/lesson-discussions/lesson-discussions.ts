import { Component, Input, OnChanges, SimpleChanges, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { DiscussionService } from '@services/discussion.service';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';
import { DiscussionResponse, DiscussionDetailResponse, ReplyResponse } from '@models/discussion';

import { Button } from '@components/button/button';
import { Loader } from '@components/loader/loader';
import { FormInput } from '@components/form-input/form-input';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';

@Component({
  selector: 'app-lesson-discussions',
  standalone: true,
  imports: [CommonModule, FormsModule, Button, Loader, FormInput, ConfirmModal],
  templateUrl: './lesson-discussions.html',
  styleUrl: './lesson-discussions.css'
})
export class LessonDiscussions implements OnChanges {
  @Input({ required: true }) lessonId!: number;

  private discussionService = inject(DiscussionService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  discussions = signal<DiscussionResponse[]>([]);
  isLoading = signal(false);

  showNewForm = signal(false);
  newTitle = signal('');
  newContent = signal('');
  isPosting = signal(false);

  selectedId = signal<number | null>(null);
  detail = signal<DiscussionDetailResponse | null>(null);
  isLoadingDetail = signal(false);

  replyText = signal('');
  isReplying = signal(false);

  editingDiscussion = signal(false);
  editTitle = signal('');
  editContent = signal('');

  editingReplyId = signal<number | null>(null);
  editReplyText = signal('');

  deleteTarget = signal<{ type: 'discussion' | 'reply'; id: number } | null>(null);

  currentUserEmail = () => this.authService.currentUser()?.email || '';

  ngOnChanges(changes: SimpleChanges) {
    if (changes['lessonId'] && this.lessonId) {
      this.loadDiscussions();
    }
  }

  private loadDiscussions() {
    this.isLoading.set(true);
    this.discussionService.getLessonDiscussions(this.lessonId).subscribe({
      next: (res) => this.discussions.set(res),
      error: (err) => this.toastService.showApiError(err, 'Failed to load discussions.'),
      complete: () => this.isLoading.set(false)
    });
  }

  toggleNewForm() {
    this.showNewForm.set(!this.showNewForm());
    this.newTitle.set('');
    this.newContent.set('');
  }

  submitNewDiscussion() {
    if (!this.newTitle().trim()) {
      this.toastService.showError('Please enter a title.');
      return;
    }
    if (!this.newContent().trim()) {
      this.toastService.showError('Please enter some details for your question.');
      return;
    }

    this.isPosting.set(true);
    this.discussionService.createDiscussion({
      lessonId: this.lessonId,
      title: this.newTitle(),
      content: this.newContent()
    }).subscribe({
      next: () => {
        this.toastService.showSuccess('Question posted.');
        this.toggleNewForm();
        this.loadDiscussions();
      },
      error: (err) => this.toastService.showApiError(err, 'Failed to post question.'),
      complete: () => this.isPosting.set(false)
    });
  }

  openDiscussion(id: number) {
    this.selectedId.set(id);
    this.isLoadingDetail.set(true);
    this.discussionService.getDiscussionDetail(id).subscribe({
      next: (res) => this.detail.set(res),
      error: (err) => this.toastService.showApiError(err, 'Failed to load discussion.'),
      complete: () => this.isLoadingDetail.set(false)
    });
  }

  closeDetail() {
    this.selectedId.set(null);
    this.detail.set(null);
    this.editingDiscussion.set(false);
    this.editingReplyId.set(null);
  }

  isOwner(email: string): boolean {
    return !!email && email === this.currentUserEmail();
  }

  // ─── Discussion Edit ────────────────────────────────────────────────────
  startEditDiscussion() {
    const d = this.detail();
    if (!d) return;
    this.editTitle.set(d.title);
    this.editContent.set(d.content);
    this.editingDiscussion.set(true);
  }

  cancelEditDiscussion() {
    this.editingDiscussion.set(false);
  }

  saveEditDiscussion() {
    const d = this.detail();
    if (!d) return;

    if (!this.editTitle().trim() || !this.editContent().trim()) {
      this.toastService.showError('Title and content cannot be empty.');
      return;
    }

    this.discussionService.updateDiscussion(d.id, {
      title: this.editTitle(),
      content: this.editContent()
    }).subscribe({
      next: () => {
        this.toastService.showSuccess('Question updated.');
        this.editingDiscussion.set(false);
        this.openDiscussion(d.id);
        this.loadDiscussions();
      },
      error: (err) => this.toastService.showApiError(err, 'Failed to update question.')
    });
  }

  // ─── Reply ──────────────────────────────────────────────────────────────
  submitReply() {
    const d = this.detail();
    if (!d) return;

    if (!this.replyText().trim()) {
      this.toastService.showError('Reply cannot be empty.');
      return;
    }

    this.isReplying.set(true);
    this.discussionService.addReply(d.id, { replyText: this.replyText() }).subscribe({
      next: () => {
        this.toastService.showSuccess('Reply posted.');
        this.replyText.set('');
        this.openDiscussion(d.id);
        this.loadDiscussions();
      },
      error: (err) => this.toastService.showApiError(err, 'Failed to post reply.'),
      complete: () => this.isReplying.set(false)
    });
  }

  startEditReply(reply: ReplyResponse) {
    this.editingReplyId.set(reply.id);
    this.editReplyText.set(reply.replyText);
  }

  cancelEditReply() {
    this.editingReplyId.set(null);
  }

  saveEditReply(replyId: number) {
    const d = this.detail();
    if (!d) return;

    if (!this.editReplyText().trim()) {
      this.toastService.showError('Reply cannot be empty.');
      return;
    }

    this.discussionService.updateReply(replyId, { replyText: this.editReplyText() }).subscribe({
      next: () => {
        this.toastService.showSuccess('Reply updated.');
        this.editingReplyId.set(null);
        this.openDiscussion(d.id);
      },
      error: (err) => this.toastService.showApiError(err, 'Failed to update reply.')
    });
  }

  // ─── Like ───────────────────────────────────────────────────────────────
  toggleLike(discussionId: number) {
    this.discussionService.toggleLike(discussionId).subscribe({
      next: (count) => {
        const d = this.detail();
        if (d && d.id === discussionId) {
          this.detail.set({ ...d, likeCount: count, isLikedByUser: !d.isLikedByUser });
        }
        this.discussions.set(this.discussions().map(x =>
          x.id === discussionId ? { ...x, likeCount: count, isLikedByUser: !x.isLikedByUser } : x
        ));
      },
      error: (err) => this.toastService.showApiError(err, 'Failed to update like.')
    });
  }

  // ─── Delete ─────────────────────────────────────────────────────────────
  requestDeleteDiscussion(id: number) {
    this.deleteTarget.set({ type: 'discussion', id });
  }

  requestDeleteReply(id: number) {
    this.deleteTarget.set({ type: 'reply', id });
  }

  cancelDelete() {
    this.deleteTarget.set(null);
  }

  confirmDelete() {
    const target = this.deleteTarget();
    if (!target) return;

    if (target.type === 'discussion') {
      this.discussionService.deleteDiscussion(target.id).subscribe({
        next: () => {
          this.toastService.showSuccess('Question deleted.');
          this.deleteTarget.set(null);
          this.closeDetail();
          this.loadDiscussions();
        },
        error: (err) => this.toastService.showApiError(err, 'Failed to delete question.')
      });
    } else {
      const d = this.detail();
      this.discussionService.deleteReply(target.id).subscribe({
        next: () => {
          this.toastService.showSuccess('Reply deleted.');
          this.deleteTarget.set(null);
          if (d) this.openDiscussion(d.id);
        },
        error: (err) => this.toastService.showApiError(err, 'Failed to delete reply.')
      });
    }
  }
}
