export type Status = "Open" | "InReview" | "Resolved";
export const statusLabels: Record<Status, string> = {
  Open: "Aberta",
  InReview: "Em análise",
  Resolved: "Resolvida",
};
export interface User {
  id: string;
  name: string;
  email: string;
  role: "Citizen" | "Manager";
}
export interface Session {
  accessToken: string;
  refreshToken: string;
  user: User;
}
export interface Category {
  id: number;
  name: string;
}
export interface OccurrenceInput {
  address?: string | null;
  categoryId: number;
  description: string;
  difficulty: number;
  latitude: number;
  longitude: number;
}
export interface Occurrence extends OccurrenceInput {
  id: string;
  authorId: string;
  category: string;
  createdAt: string;
  status: Status;
  hasPhoto: boolean;
  priority: {
    score: number;
    level: string;
    severityPoints: number;
    recurrencePoints: number;
    agePoints: number;
  };
  history: { status: Status; createdAt: string }[];
}
export interface Page<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}
export interface Indicators {
  total: number;
  categories: Count[];
  statuses: Count[];
  priorities: Count[];
  timeline: Count[];
  concentrations: { latitude: number; longitude: number; count: number }[];
}
export interface Count {
  label: string;
  count: number;
}
export interface Draft {
  key: string;
  input: OccurrenceInput;
  photo?: string;
  occurrenceId?: string;
  pending?: boolean;
  updatedAt: string;
}
