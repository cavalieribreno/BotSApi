// Operating hours for a single day of the week (matches API BusinessHoursResponse / BusinessHoursRequest)
export interface BusinessHours {
  dayOfWeek: number; // 0 = Domingo, 1 = Segunda ... 6 = Sábado
  opensAt: string;   // "09:00"
  closesAt: string;  // "19:00"
  isClosed: boolean;
}
