import { useRef, useState } from "react";
import { Text } from "react-native";
import { request, setSession } from "../src/api";
import { Session } from "../src/types";
import { Body, Button, Card, Field, Screen, styles } from "../src/ui";
export default function Login() {
  const [register, setRegister] = useState(false);
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmation, setConfirmation] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const submitting = useRef(false);
  async function submit() {
    if (submitting.current) return;
    setError("");
    const normalizedEmail = email.trim().toLowerCase();
    const normalizedName = name.trim();
    if (
      !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(normalizedEmail) ||
      password.length < 8 ||
      password.length > 100 ||
      (register && (normalizedName.length < 2 || normalizedName.length > 80))
    ) {
      setError(
        "Informe um e-mail válido, senha de 8 a 100 caracteres e nome com ao menos 2 caracteres no cadastro.",
      );
      return;
    }
    if (register && password !== confirmation) {
      setError("As senhas não conferem. Verifique a confirmação.");
      return;
    }
    submitting.current = true;
    setBusy(true);
    try {
      await setSession(
        await request<Session>(`/auth/${register ? "register" : "login"}`, {
          method: "POST",
          body: JSON.stringify(
            register
              ? { name: normalizedName, email: normalizedEmail, password }
              : { email: normalizedEmail, password },
          ),
        }),
      );
    } catch (e) {
      setError((e as Error).message);
    } finally {
      submitting.current = false;
      setBusy(false);
    }
  }
  return (
    <Screen title="Passo Seguro">
      <Text style={styles.subtitle}>
        Sua experiência ajuda a tornar a cidade mais acessível.
      </Text>
      <Body>
        Registre obstáculos, acompanhe ocorrências e conheça as dificuldades de
        circulação em Bela Vista.
      </Body>
      <Card>
        <Text style={styles.subtitle}>
          {register ? "Criar conta" : "Entrar"}
        </Text>
        {register && (
          <Field
            label="Nome"
            value={name}
            onChangeText={setName}
            autoComplete="name"
            maxLength={80}
            editable={!busy}
          />
        )}
        <Field
          label="E-mail"
          value={email}
          onChangeText={setEmail}
          autoCapitalize="none"
          keyboardType="email-address"
          autoComplete="email"
          autoCorrect={false}
          maxLength={200}
          editable={!busy}
        />
        <Field
          label="Senha"
          value={password}
          onChangeText={setPassword}
          secureTextEntry
          autoComplete={register ? "new-password" : "current-password"}
          maxLength={100}
          editable={!busy}
        />
        {register && (
          <Field
            label="Confirmar senha"
            value={confirmation}
            onChangeText={setConfirmation}
            secureTextEntry
            autoComplete="new-password"
            maxLength={100}
            editable={!busy}
          />
        )}
        {!!error && (
          <Text accessibilityRole="alert" style={styles.error}>
            {error}
          </Text>
        )}
        <Button
          title={busy ? "Aguarde…" : register ? "Criar minha conta" : "Entrar"}
          onPress={submit}
          disabled={busy}
        />
        <Button
          title={register ? "Já tenho conta" : "Criar conta"}
          secondary
          disabled={busy}
          onPress={() => {
            setRegister(!register);
            setError("");
            setConfirmation("");
          }}
        />
      </Card>
      <Text style={styles.muted}>
        Conecte-se para registrar obstáculos e acompanhar a acessibilidade do
        seu bairro.
      </Text>
    </Screen>
  );
}
