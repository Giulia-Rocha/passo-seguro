import { useCallback, useState } from 'react';
import { useFocusEffect } from 'expo-router';
import { ApiError, request } from './api';
import { storage } from './storage';
import { useSession } from './session';
export function useResource<T>(path: string) {
  const {session}=useSession(); const owner=session?.user.id ?? '';
  const [data,setData]=useState<T|null>(null); const [loading,setLoading]=useState(true);
  const [error,setError]=useState(''); const [cachedAt,setCachedAt]=useState('');
  const load=useCallback(async()=> {
    if(!owner) return;
    setLoading(true); setError(''); setCachedAt('');
    try { const value=await request<T>(path); setData(value); await storage.cache(owner,path,value); }
    catch(e) { setError(e instanceof Error?e.message:'Falha ao consultar.'); if(e instanceof ApiError && e.status===0) { const cached=await storage.read<T>(owner,path); if(cached) { setData(cached.value); setCachedAt(cached.updated); } else setData(null); } else setData(null); }
    finally { setLoading(false); }
  },[owner,path]);
  useFocusEffect(useCallback(()=> { void load(); },[load]));
  return {data,loading,error,cachedAt,reload:load};
}
