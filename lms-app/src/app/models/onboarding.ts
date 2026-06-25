export interface CreateLinkedAccountRequest {
  email: string;
  phone: string;
  legalBusinessName: string;
  contactName: string;
  businessType: string;
  profileCategory: string;
  profileSubcategory: string;
  street1: string;
  street2?: string | null;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  pan?: string | null;
  gst?: string | null;
}

export interface UpdateLinkedAccountRequest {
  email: string;
  phone: string;
  legalBusinessName: string;
  contactName: string;
  profileCategory: string;
  profileSubcategory: string;
  street1: string;
  street2?: string | null;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  pan?: string | null;
  gst?: string | null;
}

export interface CreateStakeholderRequest {
  name: string;
  email: string;
}

export interface UpdateStakeholderRequest {
  name: string;
  email: string;
}

export interface ConfigureBankRequest {
  accountNumber: string;
  ifscCode: string;
  beneficiaryName: string;
}

export interface LinkedAccountResponse {
  id: number;
  razorpayAccountId: string;
  legalBusinessName: string;
  contactName: string;
  email: string;
  phone: string;
  businessType: string;
  accountStatus: string;
  isActive: boolean;
  isVerified: boolean;
  createdAt: string;
  updatedAt: string;
  hasStakeholder: boolean;
  hasProduct: boolean;
  isBankConfigured: boolean;
}

export interface StakeholderResponse {
  id: number;
  razorpayStakeholderId: string;
  name: string;
  email: string;
  createdAt: string;
}

export interface PayoutProductResponse {
  id: number;
  razorpayProductId: string;
  productStatus: string;
  tncAccepted: boolean;
  accountNumber: string;
  ifscCode: string;
  beneficiaryName: string;
  createdAt: string;
  updatedAt: string;
}

export interface OnboardingStatusResponse {
  currentStep: string;
  accountStatus: string;
  account?: LinkedAccountResponse | null;
  stakeholder?: StakeholderResponse | null;
  product?: PayoutProductResponse | null;
}
