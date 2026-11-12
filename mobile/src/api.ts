import * as SecureStore from 'expo-secure-store';
import { Session } from './types';
export const API_URL = process.env.EXPO_PUBLIC_API_URL?.replace(/\/$/, '') ?? 'http://localhost:5080/api/v1';
export class ApiError extends Error { constructor(message: string, public status: number) { super(message); } }
let session: Session | null = null;
let refreshing: Promise<void> | null = null;
let sessionListener: (session: Session | null)=>void = ()=>{};
export function listenSession(listener: typeof sessionListener) { sessionListener=listener; return ()=>{ sessionListener=()=>{}; }; }
export async function setSession(value: Session | null) {
  session=value;
  if(value) await SecureStore.setItemAsync('passoseguro.session',JSON.stringify(value)); else await SecureStore.deleteItemAsync('passoseguro.session');
  sessionListener(value);
}
export async function restoreSession() { const saved=await SecureStore.getItemAsync('passoseguro.session'); if(saved) { try { session=JSON.parse(saved); } catch { await setSession(null); } } return session; }
export function currentSession() { return session; }
async function perform(path: string, options: RequestInit, retry=true): Promise<Response> {
  const headers=new Headers(options.headers);
  if(!(options.body instanceof FormData)) headers.set('Content-Type','application/json');
  if(session) headers.set('Authorization',`Bearer ${session.accessToken}`);
  let response: Response;
  try { response=await fetch(`${API_URL}${path}`,{...options,headers,signal:AbortSignal.timeout(20000)}); }
  catch { throw new ApiError('Não foi possível acessar o servidor. Verifique sua conexão e tente novamente.',0); }
  if(response.status===401 && retry && session && !path.startsWith('/auth/')) {
    if(!refreshing) refreshing=(async()=> {
      const result=await perform('/auth/refresh',{method:'POST',body:JSON.stringify({refreshToken:session!.refreshToken})},false);
      if(!result.ok) { if(result.status===401) await setSession(null); throw new ApiError('Sua sessão expirou. Entre novamente.',result.status); }
      await setSession(await result.json());
    })().finally(()=> { refreshing=null; });
    await refreshing;
    return perform(path,options,false);
  }
  return response;
}
export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response=await perform(path,options);
  if(!response.ok) { const problem=await response.json().catch(()=>({})); throw new ApiError(problem.detail ?? problem.title ?? `Falha na operação (${response.status}).`,response.status); }
  return response.status===204 ? undefined as T : response.json();
}
export async function uploadPhoto(id: string, uri: string) {
  const form=new FormData();
  const png=uri.toLowerCase().endsWith('.png');
  form.append('file',{uri,name:png?'foto.png':'foto.jpg',type:png?'image/png':'image/jpeg'} as unknown as Blob);
  return request<void>(`/occurrences/${id}/photo`,{method:'PUT',body:form});
}
