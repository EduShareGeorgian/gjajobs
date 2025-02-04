package com.georgiancollege;

import jakarta.inject.Singleton;
import java.nio.file.*;
import java.io.IOException;
import java.sql.*;
import java.net.InetAddress;
import java.net.UnknownHostException;
import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.Stream;
import java.util.regex.Pattern;

@Singleton
public class GjajobsService {

    // Variables from VB code
    private String logFile;
    private String reportsFolder;

    public void init() throws IOException {
        reportsFolder = System.getenv("REPORTS_FOLDER");
        if (reportsFolder == null) reportsFolder = "reports";
        Path logPath = Paths.get("logs");
        if (!Files.exists(logPath)) {
            Files.createDirectories(logPath);
        }
        logFile = "logs/gjajobs.log";
        Files.writeString(Paths.get(logFile), "Init GJAJOBS\n", 
                StandardOpenOption.CREATE, StandardOpenOption.APPEND);
    }

    public void submit(String job, String jobType, String user, String pwd, String seqno, String printer, String formName, String submitTime, String completionTime) {
        putLog("Submitting " + job + " for " + user);

        String url = "jdbc:oracle:thin:@//localhost:1521/orcl";
        String username = "your_db_username";
        String password = "your_db_password";

        String insertSQL = "INSERT INTO GWJSUBQ (gwjsubq_Server, gwjsubq_Status, gwjsubq_Service, gwjsubq_Job, gwjsubq_Seq, gwjsubq_User, gwjsubq_Submitted, gwjsubq_SID, gwjsubq_P1, gwjsubq_P2, gwjsubq_P3, gwjsubq_P4, gwjsubq_P5, gwjsubq_P6, gwjsubq_P7, gwjsubq_P8, gwjsubq_P9) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

        try (Connection conn = DriverManager.getConnection(url, username, password);
             PreparedStatement pstmt = conn.prepareStatement(insertSQL)) {

            pstmt.setString(1, InetAddress.getLocalHost().getHostName().toUpperCase());
            pstmt.setString(2, " ");
            String oracleSid = System.getenv("ORACLE_SID");
            if (oracleSid != null) {
                pstmt.setString(3, oracleSid.substring(0, 4).toUpperCase());
                pstmt.setString(8, oracleSid.toUpperCase());
            } else {
                pstmt.setString(3, "DEFAULT");
                pstmt.setString(8, "DEFAULT");
            }
            pstmt.setString(4, job.toUpperCase());
            pstmt.setInt(5, Integer.parseInt(seqno));
            pstmt.setString(6, user.toUpperCase());
            pstmt.setTimestamp(7, new Timestamp(System.currentTimeMillis()));
            pstmt.setString(9, job);
            pstmt.setString(10, jobType);
            pstmt.setString(11, user);
            pstmt.setString(12, encrypt(pwd));
            pstmt.setString(13, seqno);
            pstmt.setString(14, printer);
            pstmt.setString(15, formName);
            pstmt.setString(16, submitTime);
            pstmt.setString(17, completionTime);

            pstmt.executeUpdate();
        } catch (SQLException | UnknownHostException e) {
            putLog("Error submitting job: " + e.getMessage());
        }
    }

    public void sanitize(String job, String user, String pwd, String seqno) {
        putLog("Sanitizing " + job + " for " + user);

        String oracleSid = System.getenv("ORACLE_SID");
        if (oracleSid == null) {
            putLog("ORACLE_SID environment variable is not set.");
            return;
        }

        Path logFilePath = Paths.get(reportsFolder, oracleSid.substring(0, 4), user, job + "_" + seqno + ".log");
        Path batFilePath = Paths.get(reportsFolder, oracleSid.substring(0, 4), user, job + "_" + seqno + ".bat");
        Path prmFilePath = Paths.get(reportsFolder, oracleSid.substring(0, 4), user, job + "_" + seqno + ".prm");

        try {
            sanitizeFile(logFilePath, pwd);
            sanitizeFile(batFilePath, pwd);
            sanitizeFile(prmFilePath, pwd);
            renameReports(Paths.get(reportsFolder, oracleSid.substring(0, 4), user));
        } catch (IOException e) {
            putLog("Error sanitizing job: " + e.getMessage());
        }
    }

    private void sanitizeFile(Path filePath, String pwd) throws IOException {
        if (Files.exists(filePath)) {
            List<String> lines = Files.readAllLines(filePath);
            List<String> sanitizedLines = lines.stream()
                    .map(line -> line.replaceAll("(?i)" + Pattern.quote(pwd), "*********"))
                    .collect(Collectors.toList());
            Files.write(filePath, sanitizedLines);
        } else {
            putLog("File not found: " + filePath.toString());
        }
    }

    private void renameReports(Path startFolder) throws IOException {
        try (Stream<Path> files = Files.walk(startFolder)) {
            files.filter(Files::isRegularFile)
                 .filter(file -> file.toString().toLowerCase().endsWith(".lis") || file.toString().toLowerCase().endsWith(".rpt"))
                 .forEach(file -> {
                     try {
                         Path newFile = Paths.get(file.toString().replaceAll("\\.lis$", ".txt").replaceAll("\\.rpt$", ".txt"));
                         if (!Files.exists(newFile)) {
                             Files.move(file, newFile);
                         }
                     } catch (IOException e) {
                         putLog("Error renaming file: " + e.getMessage());
                     }
                 });
        }
    }

    public void complete(String job, String jobType, String user, String pwd, String seqno, String printer, String formName, String submitTime, String completionTime) {
        putLog("Completing " + job + " for " + user);

        String url = "jdbc:oracle:thin:@//localhost:1521/orcl";
        String username = "your_db_username";
        String password = "your_db_password";

        String updateSQL = "UPDATE GWJSUBQ SET gwjsubq_Status = ?, gwjsubq_Message = ?, gwjsubq_Completed = ?, gwjsubq_SID = ?, gwjsubq_P1 = ?, gwjsubq_P2 = ?, gwjsubq_P3 = ?, gwjsubq_P4 = ?, gwjsubq_P5 = ?, gwjsubq_P6 = ?, gwjsubq_P7 = ?, gwjsubq_P8 = ?, gwjsubq_P9 = ? WHERE gwjsubq_Service = ? AND gwjsubq_Job = ? AND gwjsubq_Seq = ? AND gwjsubq_User = ?";

        try (Connection conn = DriverManager.getConnection(url, username, password);
             PreparedStatement pstmt = conn.prepareStatement(updateSQL)) {

            pstmt.setString(1, "X");
            pstmt.setString(2, completionTime);
            pstmt.setTimestamp(3, new Timestamp(System.currentTimeMillis()));
            String oracleSid = System.getenv("ORACLE_SID");
            if (oracleSid != null) {
                pstmt.setString(4, oracleSid.toUpperCase());
                pstmt.setString(14, oracleSid.substring(0, 4).toUpperCase());
            } else {
                pstmt.setString(4, "DEFAULT");
                pstmt.setString(14, "DEFAULT");
            }
            pstmt.setString(5, job);
            pstmt.setString(6, jobType);
            pstmt.setString(7, user);
            pstmt.setString(8, encrypt(pwd));
            pstmt.setString(9, seqno);
            pstmt.setString(10, printer);
            pstmt.setString(11, formName);
            pstmt.setString(12, submitTime);
            pstmt.setString(13, completionTime);
            pstmt.setString(15, job.toUpperCase());
            pstmt.setInt(16, Integer.parseInt(seqno));
            pstmt.setString(17, user.toUpperCase());

            pstmt.executeUpdate();
        } catch (SQLException e) {
            putLog("Error completing job: " + e.getMessage());
        }
    }

    public void putLog(String msg) {
        String file = (logFile != null) ? logFile : "logs/gjajobs.log";
        try {
            Files.writeString(Paths.get(file), msg + System.lineSeparator(),
                    StandardOpenOption.CREATE, StandardOpenOption.APPEND);
        } catch (IOException e) {
            // fallback
        }
    }

    private String encrypt(String pwd) {
        // Placeholder for encryption logic
        return pwd; // Replace with actual encryption logic
    }
}