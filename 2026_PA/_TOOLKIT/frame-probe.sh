#!/usr/bin/env bash
# =====================================================================
#  frame-probe.sh - TCP-Test im ECHTEN Transfer<T>-Format:
#  [4-Byte-Laenge little-endian][UTF-8-XML]. (net-probe sendet OHNE Laenge!)
#
#  Client:       ./frame-probe.sh <host> <port> [--root MSG] [--field k=v]... [--xml '<...>']
#  Echo-Server:  ./frame-probe.sh --listen <port>
#  Beispiel:     ./frame-probe.sh localhost 12345 --root MSG --field type=SEARCH --field Search=Anna
# =====================================================================
command -v python3 >/dev/null 2>&1 || { echo "FEHLER: python3 nicht gefunden." >&2; exit 1; }
exec python3 -u - "$@" <<'PY'
import sys, socket, struct

args = sys.argv[1:]

def frame(b):
    return struct.pack('<i', len(b)) + b           # 4-Byte-Laenge LITTLE-ENDIAN + Daten

def recv_exactly(sock, n):
    buf = b''
    while len(buf) < n:
        chunk = sock.recv(n - len(buf))
        if not chunk:
            return None
        buf += chunk
    return buf

def read_frame(sock):
    hdr = recv_exactly(sock, 4)
    if hdr is None:
        return None
    (length,) = struct.unpack('<i', hdr)
    return recv_exactly(sock, length)

if args and args[0] == '--listen':
    port = int(args[1])
    srv = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    srv.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    srv.bind(('0.0.0.0', port)); srv.listen(5)
    print("frame-probe Echo-Server laeuft auf Port %d (Strg+C beendet)" % port)
    while True:
        conn, addr = srv.accept()
        print("-- Client verbunden: %s" % (addr,))
        try:
            while True:
                data = read_frame(conn)
                if data is None:
                    break
                print("<- empfangen:", data.decode('utf-8', 'replace'))
                conn.sendall(frame(data))          # gerahmt zurueckschicken
        except Exception as e:
            print("Fehler:", e)
        conn.close(); print("-- Client getrennt")
else:
    if len(args) < 2:
        print("Verwendung: frame-probe.sh <host> <port> [--root MSG] [--field k=v]... | --listen <port>")
        sys.exit(1)
    host, port = args[0], int(args[1])
    root, fields, raw, i = 'MSG', [], None, 2
    while i < len(args):
        a = args[i]
        if a == '--root':   root = args[i+1]; i += 2
        elif a == '--field':
            k, _, v = args[i+1].partition('='); fields.append((k, v)); i += 2
        elif a == '--xml':  raw = args[i+1]; i += 2
        else: i += 1
    if raw is None:
        body = ''.join("<%s>%s</%s>" % (k, v, k) for k, v in fields)
        raw = "<%s>%s</%s>" % (root, body, root)
    payload = raw.encode('utf-8')
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM); s.settimeout(3)
    s.connect((host, port))
    print("-> sende (%d Bytes XML): %s" % (len(payload), raw))
    s.sendall(frame(payload))
    try:
        reply = read_frame(s)
        if reply is None:
            print("<- keine gerahmte Antwort / Verbindung geschlossen")
        else:
            print("<- Antwort:", reply.decode('utf-8', 'replace'))
    except socket.timeout:
        print("<- Timeout (keine Antwort in 3s)")
    s.close()
PY
