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
