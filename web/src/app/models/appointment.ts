// Shape of an appointment returned by GET /api/appointments (matches the API's AppointmentResponse).
export interface Appointment {
  id: string;
  companyId: string;
  conversationId: string | null;
  professionalId: string;
  professionalName: string;
  serviceName: string;
  customerName: string;
  customerPhone: string;
  scheduledAt: string;
  status: string;
  origin: string;
  createdAt: string;
}

// Request payload for manual appointment creation (POST /api/appointments).
export interface CreateAppointmentRequest {
  professionalId: string;
  serviceName: string;
  customerName: string;
  customerPhone: string;
  scheduledAt: string;
}
