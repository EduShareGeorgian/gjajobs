package com.georgiancollege;

import io.micronaut.configuration.picocli.PicocliRunner;
import picocli.CommandLine.Command;
import picocli.CommandLine.Option;

import jakarta.inject.Inject;
import java.nio.file.*;
import java.io.IOException;
import java.util.List;

@Command(name = "gjajobs", description = "Migrated GJAJOBS", mixinStandardHelpOptions = true)
public class GjajobsCommand implements Runnable {

    @Option(names = {"--job"}, description = "Job name")
    private String job;

    @Option(names = {"--action"}, description = "submit|sanitize|complete")
    private String action;

    @Option(names = {"--jobType"}, description = "Job type")
    private String jobType;

    @Option(names = {"--user"}, description = "User name")
    private String user;

    @Option(names = {"--pwd"}, description = "Password")
    private String pwd;

    @Option(names = {"--seqno"}, description = "Sequence number")
    private String seqno;

    @Option(names = {"--printer"}, description = "Printer name")
    private String printer;

    @Option(names = {"--formName"}, description = "Form name")
    private String formName;

    @Option(names = {"--submitTime"}, description = "Submit time")
    private String submitTime;

    @Option(names = {"--completionTime"}, description = "Completion time")
    private String completionTime;

    @Inject
    GjajobsService service; // A service class replicating Main_Oracle, Main_Access, Put_Log, etc.

    public static void main(String[] args) {
        PicocliRunner.run(GjajobsCommand.class, args);
    }

    @Override
    public void run() {
        // Parameter checks, environment variables, etc.
        try {
            service.init(); // set up logs, environment, etc.
            switch ((action == null) ? "submit" : action.toLowerCase()) {
                case "sanitize":
                    service.sanitize(job, user, pwd, seqno);
                    break;
                case "complete":
                    service.complete(job, jobType, user, pwd, seqno, printer, formName, submitTime, completionTime);
                    break;
                default:
                    service.submit(job, jobType, user, pwd, seqno, printer, formName, submitTime, completionTime);
                    break;
            }
        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
        }
    }
}