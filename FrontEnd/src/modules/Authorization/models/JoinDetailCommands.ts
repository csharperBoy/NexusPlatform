

// src/modules/Authorization/models/JoinDetailCommands.ts

export interface CreateJoinDetailCommands {
  PermissionRuleId: string;
  JoinLocalKey: string;
  JoinForeignKey: string;
  JoinEntity: string;
}

export type UpdateJoinDetailCommands = {
  Id: string;
  PermissionRuleId?: string | null;
  JoinLocalKey?: string| null;
  JoinForeignKey?: string| null;
  JoinEntity?: string| null;
} & Partial<CreateJoinDetailCommands>;



export type JoinDetailFormCommand = CreateJoinDetailCommands | UpdateJoinDetailCommands;