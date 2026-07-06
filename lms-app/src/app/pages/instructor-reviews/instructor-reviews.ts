import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Dropdown } from '@components/dropdown/dropdown';
import { PaginationComponent } from '@components/pagination/pagination.component';
import { SearchInput } from '@components/search-input/search-input';
import { ReviewService } from '@services/review.service';
import { CourseService } from '@services/course.service';
import { PagedInstructorReviewResponse, InstructorReviewResponse } from '@models/review';
import { ToastService } from '@services/toast.service';

@Component({
  selector: 'app-instructor-reviews',
  standalone: true,
  imports: [CommonModule, Dropdown, PaginationComponent, SearchInput],
  templateUrl: './instructor-reviews.html',
  styleUrl: './instructor-reviews.css'
})
export class InstructorReviewsPage implements OnInit {
  private reviewService = inject(ReviewService);
  private courseService = inject(CourseService);
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  reviews: InstructorReviewResponse[] = [];
  totalCount = 0;
  totalPages = 0;
  averageRating = 0;
  ratingDistribution: { [key: number]: number } = { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 };

  page = 1;
  pageSize = 10;
  
  searchQuery = '';
  selectedRating = '';
  selectedCourseId = '';

  isLoading = true;

  ratingOptions = [
    { value: '', label: 'All Ratings' },
    { value: '5', label: '5 Stars' },
    { value: '4', label: '4 Stars' },
    { value: '3', label: '3 Stars' },
    { value: '2', label: '2 Stars' },
    { value: '1', label: '1 Star' }
  ];

  courseOptions: { value: string, label: string }[] = [{ value: '', label: 'All Courses' }];

  ngOnInit(): void {
    this.loadCourses();
    this.loadReviews();
  }

  loadCourses() {
    this.courseService.getMyCourses().subscribe({
      next: (res) => {
        const courses = res.courses.map(c => ({ value: c.id.toString(), label: c.title }));
        this.courseOptions = [{ value: '', label: 'All Courses' }, ...courses];
        this.cdr.detectChanges();
      }
    });
  }

  loadReviews() {
    this.isLoading = true;
    const ratingFilter = this.selectedRating ? parseInt(this.selectedRating) : undefined;
    const courseFilter = this.selectedCourseId ? parseInt(this.selectedCourseId) : undefined;
    
    this.reviewService.getInstructorReviews(this.page, this.pageSize, ratingFilter, courseFilter, this.searchQuery)
      .subscribe({
        next: (res: PagedInstructorReviewResponse) => {
          this.reviews = res.reviews;
          this.totalCount = res.totalCount;
          this.totalPages = res.totalPages;
          this.averageRating = res.averageRating;
          this.ratingDistribution = res.ratingDistribution;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error loading reviews', err);
          this.toastService.showError('Failed to load reviews');
          this.isLoading = false;
        }
      });
  }

  onSearchChange(search: string) {
    this.searchQuery = search;
    this.page = 1;
    this.loadReviews();
  }

  onRatingChange(rating: string) {
    this.selectedRating = rating;
    this.page = 1;
    this.loadReviews();
  }

  onCourseChange(courseId: string) {
    this.selectedCourseId = courseId;
    this.page = 1;
    this.loadReviews();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadReviews();
  }

  getDistributionPercentage(stars: number): number {
    if (this.totalCount === 0) return 0;
    const count = this.ratingDistribution[stars] || 0;
    return Math.round((count / this.totalCount) * 100);
  }
}
