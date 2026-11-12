import React, { createContext, useContext, useEffect, useState } from 'react';
import { listenSession, restoreSession, setSession, request } from './api';
import { storage } from './storage';
import { Session } from './types';
const Context=createContext<{session:Session|null; ready:boolean; logout:()=>Promise<void>}>({session:null,ready:false,logout:async()=>{}});
export function SessionProvider({children}:{children:React.ReactNode}) {
  const [session,update]=useState<Session|null>(null); const [ready,setReady]=useState(false);
  useEffect(()=> { const unsubscribe=listenSession(update); restoreSession().then(update).finally(()=>setReady(true)); return unsubscribe; },[]);
  async function logout() {
    if(session) { try { await request('/auth/logout',{method:'POST',body:JSON.stringify({refreshToken:session.refreshToken})}); } catch { /* Local logout must remain available offline. */ } await storage.clear(session.user.id); }
    await setSession(null);
  }
  return <Context.Provider value={{session,ready,logout}}>{children}</Context.Provider>;
}
export const useSession=()=>useContext(Context);
