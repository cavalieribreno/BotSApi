export type ProfessionalStatus = 'Inactive' | 'Active' | 'OnVacation';

export interface Professional {
  id: string;
  name: string;
  status: ProfessionalStatus;
}

export interface CreateProfessionalRequest {
  name: string;
}

export interface UpdateProfessionalRequest {
  name: string;
  status: ProfessionalStatus;
}
