export interface Todo {
  id: number;
  content: string;
}

export interface Meta {
  totalCount: number;
}

export interface DocumentModel {
  id: number;
  title: string;
  content: string;
  createdAt: Date;
  updatedAt: Date;
}
