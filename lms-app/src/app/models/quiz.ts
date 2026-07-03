export interface QuizAttemptResponse {
  id: number;
  quizId: number;
  quizTitle: string;
  courseTitle: string;
  sectionTitle: string;
  userId: number;
  obtainedScore: number;
  totalScore: number;
  isPassed: boolean;
  status: string;
  startedAt: string;
  completedAt?: string;
  questionsCount: number;
  passingPercentage: number;
}

export interface QuizOptionResponse {
  id: number;
  optionText: string;
  isCorrect: boolean;
}

export interface QuizQuestionResponse {
  id: number;
  quizId: number;
  questionText: string;
  questionType: number;
  mark: number;
  explanation?: string;
  sortOrder: number;
  options: QuizOptionResponse[];
}

export interface QuizAnswerResponse {
  id: number;
  questionId: number;
  questionText: string;
  selectedOptionId: number;
  selectedOptionText: string;
  isCorrect: boolean;
}

export interface QuizAttemptDetailResponse {
  id: number;
  quizId: number;
  quizTitle: string;
  courseTitle: string;
  sectionTitle: string;
  userId: number;
  obtainedScore: number;
  totalScore: number;
  isPassed: boolean;
  status: string;
  startedAt: string;
  completedAt?: string;
  questionsCount: number;
  passingPercentage: number;
  questions: QuizQuestionResponse[];
  answers: QuizAnswerResponse[];
}

export interface QuizResponse {
  id: number;
  courseSectionId: number;
  title: string;
  description?: string;
  timeLimit: string; // TimeSpan formatted
  totalMarks: number;
  passingPercentage: number;
  maxAttempts: number;
  order: number;
  status: number | string;
  deadlineInDays: number;
  deadlineDate?: string;
  questionCount: number;
}

export interface QuizStudentOptionResponse {
  id: number;
  optionText: string;
}

export interface QuizStudentQuestionResponse {
  id: number;
  questionText: string;
  questionType: number;
  mark: number;
  sortOrder: number;
  options: QuizStudentOptionResponse[];
}

export interface QuizStudentDetailResponse {
  id: number;
  courseSectionId: number;
  title: string;
  description?: string;
  timeLimit: string; // TimeSpan
  totalMarks: number;
  passingPercentage: number;
  maxAttempts: number;
  order: number;
  deadlineInDays: number;
  deadlineDate?: string;
  questions: QuizStudentQuestionResponse[];
}

export interface StartAttemptResponse {
  attemptId: number;
  quizId: number;
  userId: number;
  startedAt: string;
  timeLimit: string; // TimeSpan
}

export interface GetRemainingAttemptsResponse {
  quizId: number;
  remainingAttempts: number;
  maxAttempts: number;
}

export interface SubmitAnswerItem {
  questionId: number;
  selectedOptionId: number;
}

export interface SubmitQuizRequest {
  answers: SubmitAnswerItem[];
}

export interface PagedQuizAttemptResponse {
  attempts: QuizAttemptResponse[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
