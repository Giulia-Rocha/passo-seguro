import React from 'react';
import { ActivityIndicator, KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, TextInput, TextInputProps, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { theme as t } from './theme';
export const styles=StyleSheet.create({
  screen:{flex:1,backgroundColor:t.colors.background}, content:{padding:t.spacing.lg,gap:t.spacing.md,paddingBottom:t.spacing.xl*2},
  title:{fontSize:t.font.title,fontWeight:'700',color:t.colors.text}, subtitle:{fontSize:t.font.subtitle,fontWeight:'700',color:t.colors.text},
  text:{fontSize:t.font.body,color:t.colors.text,lineHeight:26}, muted:{fontSize:t.font.small,color:t.colors.muted,lineHeight:22},
  card:{padding:t.spacing.md,backgroundColor:t.colors.surface,borderRadius:t.radius.md,borderWidth:1,borderColor:t.colors.border,gap:t.spacing.sm},
  row:{flexDirection:'row',flexWrap:'wrap',gap:t.spacing.sm,alignItems:'center'},
  button:{minHeight:t.touch,paddingHorizontal:t.spacing.md,paddingVertical:t.spacing.sm,borderRadius:t.radius.sm,backgroundColor:t.colors.primary,alignItems:'center',justifyContent:'center'},
  buttonText:{fontSize:t.font.body,fontWeight:'600',color:t.colors.surface},
  secondary:{backgroundColor:t.colors.surface,borderWidth:1,borderColor:t.colors.primary}, secondaryText:{color:t.colors.primary},
  input:{minHeight:t.touch,padding:t.spacing.md,borderWidth:1,borderColor:t.colors.border,borderRadius:t.radius.sm,backgroundColor:t.colors.surface,color:t.colors.text,fontSize:t.font.body},
  error:{color:t.colors.danger,fontSize:t.font.body}, pill:{backgroundColor:t.colors.accent,padding:t.spacing.sm,borderRadius:t.radius.sm},
  stat:{fontSize:t.font.hero,fontWeight:'700',color:t.colors.primary}, map:{height:300,borderRadius:t.radius.md},
});
export function Screen({title,children}:{title:string;children:React.ReactNode}) { return <SafeAreaView edges={['bottom']} style={styles.screen}><KeyboardAvoidingView style={styles.screen} behavior={Platform.OS==='ios'?'padding':undefined}><ScrollView keyboardShouldPersistTaps="handled" contentContainerStyle={styles.content}><Text accessibilityRole="header" style={styles.title}>{title}</Text>{children}</ScrollView></KeyboardAvoidingView></SafeAreaView>; }
export function Card({children}:{children:React.ReactNode}) { return <View style={styles.card}>{children}</View>; }
export function Body({children}:{children:React.ReactNode}) { return <Text style={styles.text}>{children}</Text>; }
export function Button({title,onPress,secondary=false,disabled=false}:{title:string;onPress:()=>void;secondary?:boolean;disabled?:boolean}) { return <Pressable accessibilityRole="button" accessibilityLabel={title} accessibilityState={{disabled}} disabled={disabled} onPress={onPress} style={[styles.button,secondary&&styles.secondary,disabled&&{opacity:.5}]}><Text style={[styles.buttonText,secondary&&styles.secondaryText]}>{title}</Text></Pressable>; }
export function Field({label,...props}:TextInputProps&{label:string}) { return <View style={{gap:t.spacing.sm}}><Text style={styles.text}>{label}</Text><TextInput accessibilityLabel={label} placeholderTextColor={t.colors.muted} style={[styles.input,props.multiline&&{minHeight:120,textAlignVertical:'top'}]} {...props}/></View>; }
export function Feedback({loading,error,cachedAt,reload}:{loading:boolean;error:string;cachedAt?:string;reload:()=>void}) { return <>{loading&&<ActivityIndicator accessibilityLabel="Carregando dados" color={t.colors.primary}/>} {!!error&&<Card><Text accessibilityRole="alert" style={styles.error}>{error}</Text>{!!cachedAt&&<Body>Consulta salva em {new Date(cachedAt).toLocaleString('pt-BR')}. Os dados podem estar desatualizados.</Body>}<Button title="Tentar novamente" secondary onPress={reload}/></Card>}</>; }
