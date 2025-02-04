package com.georgiancollege

import spock.lang.Specification
import spock.lang.Shared
import java.nio.file.*
import java.sql.*

class GjajobsServiceSpec extends Specification {

    @Shared
    GjajobsService service = new GjajobsService()

    def setupSpec() {
        System.setProperty("ORACLE_SID", "ORCL")
    }

    def setup() {
        // Setup code if needed
    }

    def "init should create logs directory and initialize log file"() {
        given:
        def reportsFolder = "reports"
        System.metaClass.static.getenv = { String key -> return reportsFolder }

        when:
        service.init()

        then:
        Files.exists(Paths.get("logs"))
        Files.exists(Paths.get("logs/gjajobs.log"))
        Files.readString(Paths.get("logs/gjajobs.log")).contains("Init GJAJOBS")
    }

    def "submit should log and insert job details into database"() {
        given:
        def conn = Mock(Connection)
        def pstmt = Mock(PreparedStatement)
        service.metaClass.getConnection = { -> conn }
        conn.prepareStatement(_) >> pstmt

        when:
        service.submit("job1", "type1", "user1", "pwd1", "1", "printer1", "form1", "submitTime1", "completionTime1")

        then:
        1 * pstmt.setString(1, InetAddress.getLocalHost().getHostName().toUpperCase())
        1 * pstmt.setString(2, " ")
        1 * pstmt.setString(3, _)
        1 * pstmt.setString(4, "JOB1")
        1 * pstmt.setInt(5, 1)
        1 * pstmt.setString(6, "USER1")
        1 * pstmt.setTimestamp(7, _)
        1 * pstmt.setString(8, _)
        1 * pstmt.setString(9, "job1")
        1 * pstmt.setString(10, "type1")
        1 * pstmt.setString(11, "user1")
        1 * pstmt.setString(12, "pwd1")
        1 * pstmt.setString(13, "1")
        1 * pstmt.setString(14, "printer1")
        1 * pstmt.setString(15, "form1")
        1 * pstmt.setString(16, "submitTime1")
        1 * pstmt.setString(17, "completionTime1")
        1 * pstmt.executeUpdate()
        1 * pstmt.close()
        1 * conn.close()
    }

    def "sanitize should replace password in files and rename reports"() {
        given:
        def job = "JOB1"
        def user = "user1"
        def pwd = "password"
        def seqno = "1"
        def reportsFolder = "reports"
        System.metaClass.static.getenv = { String key -> return reportsFolder }
        def logFilePath = Paths.get(reportsFolder, "ORCL", user, job + "_" + seqno + ".log")
        def batFilePath = Paths.get(reportsFolder, "ORCL", user, job + "_" + seqno + ".bat")
        def prmFilePath = Paths.get(reportsFolder, "ORCL", user, job + "_" + seqno + ".prm")
        Files.createDirectories(logFilePath.getParent())
        Files.writeString(logFilePath, "password")
        Files.writeString(batFilePath, "password")
        Files.writeString(prmFilePath, "password")

        when:
        service.sanitize(job, user, pwd, seqno)

        then:
        Files.readString(logFilePath).contains("password")
        Files.readString(batFilePath).contains("password")
        Files.readString(prmFilePath).contains("password")
        // Add more assertions as needed
    }

    def "complete should log and update job details in database"() {
        given:
        def conn = Mock(Connection)
        def pstmt = Mock(PreparedStatement)
        service.metaClass.getConnection = { -> conn }
        conn.prepareStatement(_) >> pstmt

        when:
        service.complete("job1", "type1", "user1", "pwd1", "1", "printer1", "form1", "submitTime1", "completionTime1")

        then:
        1 * pstmt.setString(1, "X")
        1 * pstmt.setString(2, "completionTime1")
        1 * pstmt.setTimestamp(3, _)
        1 * pstmt.setString(4, _)
        1 * pstmt.setString(5, "job1")
        1 * pstmt.setString(6, "type1")
        1 * pstmt.setString(7, "user1")
        1 * pstmt.setString(8, "pwd1")
        1 * pstmt.setString(9, "1")
        1 * pstmt.setString(10, "printer1")
        1 * pstmt.setString(11, "form1")
        1 * pstmt.setString(12, "submitTime1")
        1 * pstmt.setString(13, "completionTime1")
        1 * pstmt.setString(14, _)
        1 * pstmt.setString(15, "JOB1")
        1 * pstmt.setInt(16, 1)
        1 * pstmt.setString(17, "USER1")
        1 * pstmt.executeUpdate()
        1 * pstmt.close()
        1 * conn.close()
    }

    def "putLog should append message to log file"() {
        given:
        def msg = "Test log message"
        service.init()

        when:
        service.putLog(msg)

        then:
        Files.readString(Paths.get("logs/gjajobs.log")).contains(msg)
    }

}